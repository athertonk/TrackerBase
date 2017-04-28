Option Explicit On
Option Strict On

#Region "Imports"
Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Data
Imports System.Collections
Imports System.Xml
Imports System.Xml.Linq
Imports System.Collections.Generic
Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Reflection.MethodBase
Imports System.Threading

Imports EH = ErrorHandler
Imports CF = CommonFunctions
Imports CV = CommonVariables
Imports GTA = GIS.GISToolsAddl
Imports MDL = Tracker.Model
Imports GeoAPI.Geometries
Imports NetTopologySuite
Imports NetTopologySuite.Features
Imports NetTopologySuite.Geometries
Imports NetTopologySuite.IO

#End Region

Public Class GISFieldUploadInfo
  Public ShapefileName As String = ""
  Public ShapeType As String = ""
  Public ColCount As Integer = -1
  Public RowCount As Integer = -1
  Public Columns As List(Of String)
End Class

Public Class GISFields
  Public fileName As String = ""
  Public shapeType As String
  Public colCount As Integer = -1
  Public rowCount As Integer = -1
  Public FIDCol As String = ""

End Class

Public Class UploadTools

#Region "Module variables"
  Private Const sqMtrsPerAcre As Double = 4046.8564224
  Private Shared dataSchema As String = CF.GetDataSchemaName
  Private Shared dataConn As String = CF.GetBaseDatabaseConnString
#End Region

  ''' <summary>
  ''' Only used with old database. Project id is hardwired.
  ''' </summary>
  Public Shared Function WriteTextFile(ByVal text As String, ByVal filename As String, ByRef callInfo As String) As String
    Dim fname As String = ""
    Dim localInfo As String = ""
    Try

      Dim prjFoldr As String = CF.GetProjectFolderByProjectId(1, localInfo)
      If localInfo.Contains("error") Then callInfo &= localInfo

      fname = filename & ".txt"
      fname = System.IO.Path.Combine(prjFoldr, fname)
      Dim objStreamWriter As StreamWriter
      objStreamWriter = File.CreateText(fname)
      objStreamWriter.WriteLine(text)
      objStreamWriter.Close()
    Catch ex As Exception
      callInfo &= EH.GetCallerMethod() & " error: " & ex.Message
    End Try
    Return fname
  End Function

  Public Shared Function GetShapefileInfo(ByVal fileName As String, ByRef callInfo As String) As GISFieldUploadInfo
    Dim retVal As New GISFieldUploadInfo
    Dim retColumns As New List(Of String)
    Dim localInfo As String = ""
    Try
      Dim factory As New GeometryFactory()
      Dim shapeFileDataReader As New ShapefileDataReader(fileName, factory)
      retVal.ShapefileName = Path.GetFileName(fileName)

      'Display the shapefile type
      Dim shpHeader As ShapefileHeader = shapeFileDataReader.ShapeHeader
      Dim shapeType As String = shpHeader.ShapeType.ToString
      retVal.ShapeType = shapeType

      'Display summary information about the Dbase file
      Dim header As DbaseFileHeader = shapeFileDataReader.DbaseHeader
      retVal.ColCount = header.Fields.Length
      retVal.RowCount = header.NumRecords
      Dim fldDescriptor As DbaseFieldDescriptor
      For i As Integer = 0 To header.NumFields - 1
        fldDescriptor = header.Fields(i)
        retColumns.Add(fldDescriptor.Name)
      Next

      'Close and free up any resources
      shapeFileDataReader.Close()
      shapeFileDataReader.Dispose()

      retVal.Columns = retColumns
    Catch ex As Exception
      callInfo &= EH.GetCallerMethod() & " error: " & ex.Message
    End Try
    Return retVal
  End Function

  Private Shared Function ImportNewGIS(ByVal projectFolderName As String, ByVal xmlfile As String, ByVal projectId As Long, _
                                       ByVal usrId As Guid, ByRef callInfo As String) As String
    Dim localInfo As String = ""

    Try
      Dim rowsAffected As Integer
      Dim insertFields As String = ""
      Dim insertValues As String = ""

      'Fields handling - simplify shapes for now
      rowsAffected = SimplifyLandMgmtUnitShapes(projectId, localInfo)
      'If localInfo.Contains("error") Then callInfo &= localInfo & Environment.NewLine & "(" & rowsAffected & " rows affected)"
      If rowsAffected = 0 Then callInfo &= "No LMU shapes updated." & Environment.NewLine

      localInfo = ""
    Catch ex As Exception
      callInfo &= EH.GetCallerMethod() & " error: " & ex.Message
    End Try
    Return localInfo
  End Function

  Public Shared Function ProcessUploadedFile(ByVal projectId As Long, ByVal projectFolderName As String, _
                                             ByVal uploadedFile As String, ByVal usrId As Guid) As String
    Dim retVal As String = ""
    Dim localInfo As String = ""
    Dim xmlFileName As String = ""
    Try
      Dim datumCount As Integer = mdl.projectdatumhelper.DeleteAllProjectDatumRecordsByProjectId(projectId, localInfo)
      If localInfo.Contains("error") Then retVal &= Environment.NewLine & localInfo & Environment.NewLine & "(datumCount: " & datumCount & ")" : Exit Try

      CF.DeleteProjectFiles(projectFolderName, localInfo)
      If localInfo.Contains("error") Then retVal &= Environment.NewLine & localInfo : Exit Try

      CF.CreateBaseProjectFolders(projectFolderName, localInfo)
      If localInfo.Contains("error") Then retVal &= Environment.NewLine & localInfo : Exit Try

      If uploadedFile.EndsWith(".zip") Then
        If Not CF.Unzip(uploadedFile, projectFolderName, localInfo) Then
          retVal &= Environment.NewLine & localInfo : Exit Try
        End If
      End If

      xmlFileName = CF.MoveUploadFilesIntoProjectFolders(projectFolderName, uploadedFile, localInfo)
      If localInfo.Contains("error") Then retVal &= Environment.NewLine & localInfo : Exit Try

      retVal &= Environment.NewLine & String.Format("xmlfile: {0}", xmlFileName)
      If xmlFileName <> "" Then

        'GIS shape handling
        Dim gisXmlFileName As String = Path.Combine(projectFolderName, CV.ProjectSupportFolder)
        gisXmlFileName = Path.Combine(gisXmlFileName, xmlFileName.Replace(".mmp.", ".gis."))
        Dim gisFileExists As Boolean = File.Exists(gisXmlFileName)
        retVal &= Environment.NewLine & gisXmlFileName & Environment.NewLine & gisFileExists.ToString & Environment.NewLine
        If gisFileExists Then
          ImportNewGIS(projectFolderName, gisXmlFileName, projectId, usrId, localInfo)
          If localInfo.Contains("error") Then retVal &= Environment.NewLine & localInfo
        End If
      End If

    Catch ex As Exception
      retVal &= EH.GetCallerMethod() & " error: " & ex.Message
    End Try
    Return retVal
  End Function

#Region "ProjectDatum-related Delete, Create or Update"

  Public Shared Function DeleteRowsThatMatchDatumId(ByVal datumID As Integer) As String
    Dim retVal As String = ""
    Try
      Dim tablist() As String = CV.DeleteProjectDatumTables
      Dim recsAffected As Integer = 0

      For w As Integer = 0 To UBound(tablist)
        Using conn As New SqlConnection(dataConn)
          Using cmd As SqlCommand = conn.CreateCommand()
            cmd.CommandText = "Delete from " & dataSchema & "." & tablist(w) & " where ObjectID = " & datumID & ""
            If conn.State = ConnectionState.Closed Then conn.Open()
            recsAffected += cmd.ExecuteNonQuery()
          End Using
        End Using
      Next
    Catch ex As Exception
      retVal &= EH.GetCallerMethod() & " error: " & ex.Message
    End Try
    Return retVal
  End Function

  Public Shared Function DeleteProjectDatumRecords(ByVal projectId As Long) As String
    Dim retVal As String = ""
    Try
      Using conn As New SqlConnection(dataConn)
        Using cmd As SqlCommand = conn.CreateCommand()
          cmd.CommandText = "Delete from " & dataSchema & ".ProjectDatum where ProjectId = " & projectId & ""
          If conn.State = ConnectionState.Closed Then conn.Open()
          cmd.ExecuteNonQuery()
        End Using
      End Using
    Catch ex As Exception
      retVal &= EH.GetCallerMethod() & " error: " & ex.Message
    End Try
    Return retVal
  End Function

#End Region

#Region "Record Creation from XML (GIS)"

  Public Shared Function SimplifyLandMgmtUnitShapes(ByVal projectId As Long, ByRef callInfo As String) As Integer
    'For now (8/5/11) want to pull out all LMU shapes and get the largest outer shell for each shape, then rewrite the wkb. Then pull them out again and clip them, again rewrite wkb.

    Dim localInfo As String = ""
    Dim retVal As Integer
    Try
      Dim lmuOids(0) As Integer
      Dim lmuShapes(0) As String
      Dim lmuAreas(0) As Double
      Dim indx As Integer

      Dim cmdText As String = "SELECT LMU.* FROM " & dataSchema & ".LandManagementUnit AS LMU INNER JOIN " & dataSchema & ".ProjectDatum AS PD " & _
              " ON LMU.ObjectId=PD.ObjectId WHERE PD.ProjectId=" & projectId
      Using conn As New SqlConnection(dataConn)
        Using cmd As SqlCommand = conn.CreateCommand()
          cmd.CommandText = cmdText

          If conn.State = ConnectionState.Closed Then conn.Open()
          Using readr As SqlDataReader = cmd.ExecuteReader
            While readr.Read
              indx = UBound(lmuOids)
              If lmuOids(indx) > 1 Then
                indx += 1
                ReDim Preserve lmuOids(indx)
                ReDim Preserve lmuShapes(indx)
                ReDim Preserve lmuAreas(indx)
              End If
              lmuOids(indx) = CInt(readr("ObjectId"))
              lmuShapes(indx) = readr("Shape").ToString
            End While
          End Using

          For indx = 0 To UBound(lmuShapes)
            lmuShapes(indx) = GetShellWkb(lmuShapes(indx), localInfo)
            If localInfo.ToLower.Contains("error") Then callInfo &= localInfo
            If lmuShapes(indx) <> "" Then lmuAreas(indx) = GTA.GetAreaForWkb(lmuShapes(indx), localInfo) / sqMtrsPerAcre
          Next

          'Put the new shapes back in db
          'Dim cmdText As String = "" 'full command text
          Dim cmdTextWhenShape As String = "" 'command text for conditionals part of update
          Dim cmdTextWhenArea As String = "" 'command text for conditionals part of update
          Dim cmdTextIn As String = "" 'list of objectids to update
          For indx = 0 To UBound(lmuShapes)
            cmdTextWhenShape &= " WHEN " & lmuOids(indx) & " THEN '" & lmuShapes(indx) & "' "
            cmdTextWhenArea &= " WHEN " & lmuOids(indx) & " THEN '" & lmuAreas(indx) & "' "
            cmdTextIn &= lmuOids(indx) & "," 'trim last comma below
          Next

          cmdText = "UPDATE " & dataSchema & ".LandManagementUnit "
          cmdText &= "   SET Shape = CASE ObjectId "
          cmdText &= cmdTextWhenShape
          cmdText &= "   END "
          cmdText &= "   , TotalArea = CASE ObjectId "
          cmdText &= cmdTextWhenArea
          cmdText &= "   END "
          cmdText &= "WHERE ObjectId IN (" & cmdTextIn.TrimEnd(","c) & ")"
          cmd.CommandText = cmdText

          'callInfo &= "     cmd: " & cmdText.Replace("WHEN", "\nWHEN") & "      "
          Dim affectedRecCnt As Integer = cmd.ExecuteNonQuery
          callInfo &= "    updated: " & affectedRecCnt & " |||  "
        End Using
      End Using

    Catch ex As Exception
      callInfo &= EH.GetCallerMethod() & " error: " & ex.Message
    Finally
    End Try
    Return retVal
  End Function

  Private Shared Function GetShellWkb(origWkb As String, ByRef callInfo As String) As String
    'Get wkb for largest shell of original wkb 
    Dim retVal As String = ""
    Try
      retVal = GTA.GetShellWkb(origWkb, callInfo)

    Catch ex As Exception
      callInfo &= EH.GetCallerMethod() & " error: " & ex.Message
    End Try
    Return retVal
  End Function

#End Region

End Class