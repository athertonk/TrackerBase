Option Explicit On
Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.Reflection.MethodBase
Imports EH = ErrorHandler
Imports CF = CommonFunctions
Imports CV = CommonVariables

Namespace Tracker.Model

  Public Class User
    Public UserId As Guid = Guid.Empty
    Public UserName As String = ""
    Public Email As String = ""
  End Class

  Public Class UserList
    Public Users As New List(Of User)
  End Class

  Public Class UserHelper

    Private Shared dataConn As String = CF.GetBaseDatabaseConnString
    Private Shared dataSchema As String = CV.ProjectProductionSchema
    Private Shared aspConn As String = CF.GetNetDatabaseConnString

    Public Function Delete(ByVal usrId As Guid, ByRef callInfo As String) As Boolean
      Return False
    End Function

#Region "Fetch"

    ''' <summary>
    ''' Returns User info from user table(s)
    ''' </summary>
    Public Shared Function GetCurrentUser(ByRef callInfo As String) As User
      Dim retVal As New User
      Dim localInfo As String = ""
      Try
        Dim features As DataTable
        Dim name As String = HttpContext.Current.User.Identity.Name

        Try
          localInfo = ""
          features = GetTable(localInfo)
          If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
        Catch ex As Exception
          Throw New Exception("GetTable (" & callInfo & ")", ex)
        End Try

        If features Is Nothing OrElse features.Rows.Count < 1 Then
          callInfo &= "GetCurrentUser error: no rows found."
          CF.SendOzzy(EH.GetCallerMethod(), "GetCurrentUser error: no rows found.", Nothing)
          Return retVal
        End If

        Dim found() As DataRow = features.Select("UserName = '" & name & "'")
        If found.Count > 0 Then
          Try
            localInfo = ""
            retVal = ExtractFromRow(found(0), localInfo)
            If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
          Catch ex As Exception
            Throw New Exception("Extract (" & callInfo & ")", ex)
          End Try
        Else
          callInfo &= "GetCurrentUser error: " & name & " not found."
        End If

      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1} ({2})", EH.GetCallerMethod(), ex.Message, ex.InnerException.Message)
        CF.SendOzzy(EH.GetCallerMethod(), ex.ToString, Nothing)
      End Try
      Return retVal
    End Function

    ''' <summary>
    ''' Return a user with the input guid.
    ''' </summary>
    Public Shared Function Fetch(ByVal guid As Guid, ByRef callInfo As String) As User
      Dim retVal As New User
      Dim localInfo As String = ""
      Try
        Dim features As DataTable

        Try
          localInfo = ""
          features = GetTable(localInfo)
          If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
        Catch ex As Exception
          Throw New Exception("GetTable (" & callInfo & ")", ex)
        End Try

        Dim found() As DataRow = features.Select("UserId = '" & guid.ToString & "'")
        If found.Count > 0 Then
          Try
            localInfo = ""
            retVal = ExtractFromRow(found(0), localInfo)
            If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
          Catch ex As Exception
            Throw New Exception("Extract (" & callInfo & ")", ex)
          End Try
        End If

      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1} ({2})", EH.GetCallerMethod(), ex.Message, ex.InnerException.Message)
      End Try
      Return retVal
    End Function

    ''' <summary>
    ''' Return a user with the input user name.
    ''' </summary>
    Public Shared Function Fetch(ByVal name As String, ByRef callInfo As String) As User
      Dim retVal As New User
      Dim localInfo As String = ""
      Try
        Dim features As DataTable

        Try
          localInfo = ""
          features = GetTable(localInfo)
          If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
        Catch ex As Exception
          Throw New Exception("GetTable (" & callInfo & ")", ex)
        End Try

        Dim found() As DataRow = features.Select("UserName = '" & name & "'")
        If found.Count > 0 Then
          Try
            localInfo = ""
            retVal = ExtractFromRow(found(0), localInfo)
            If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
          Catch ex As Exception
            Throw New Exception("Extract (" & callInfo & ")", ex)
          End Try
        End If

      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1} ({2})", EH.GetCallerMethod(), ex.Message, ex.InnerException.Message)
      End Try
      Return retVal
    End Function

    ''' <summary>
    ''' Return a list of application users.
    ''' </summary>
    Public Shared Function Fetch(ByRef callInfo As String) As UserList
      Dim retVal As New UserList
      Dim usrs As New List(Of User)
      Dim usr As New User
      Dim localInfo As String = ""
      Try
        Dim features As DataTable

        Try
          localInfo = ""
          features = GetTable(localInfo)
          If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
        Catch ex As Exception
          Throw New Exception("User Table (" & callInfo & ")", ex)
        End Try

        For Each dr As DataRow In features.Rows
          Try
            localInfo = ""
            usr = ExtractFromRow(dr, localInfo)
            If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
          Catch ex As Exception
            Throw New Exception("User (" & callInfo & ")", ex)
          End Try
          If usr IsNot Nothing Then usrs.Add(usr)
        Next

        retVal.Users = usrs

      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1} ({2})", EH.GetCallerMethod(), ex.Message, ex.InnerException.Message)
      End Try
      Return retVal
    End Function

    ''' <summary>
    ''' Get all site users.
    ''' </summary>
    Private Shared Function GetTable(ByRef callInfo As String) As DataTable
      Dim retVal As New DataTable
      Dim cmdText As String = ""
      Try
        Dim localInfo As String = ""
        Dim dbName As String = CF.GetAspNetDatabaseName() 
        Dim guid As Guid = CV.AppGuid
        cmdText = String.Format(<a>
            SELECT NET.[UserId], NET.UserName, MEM.Email
            FROM {0}.[dbo].[aspnet_Users] NET
            INNER JOIN {0}.[dbo].[aspnet_Membership] MEM ON MEM.UserId = NET.UserId
            WHERE MEM.ApplicationId = @appId 
                              </a>.Value, dbName)
        Dim parameter As New SqlParameter("@appId", SqlDbType.UniqueIdentifier)
        parameter.Value = guid

        localInfo = ""
        retVal = CF.GetDataTable(aspConn, cmdText, parameter, localInfo)
        If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo

      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1}", EH.GetCallerMethod(), ex.Message)
      End Try
      Return retVal
    End Function

    Private Shared Function ExtractFromRow(ByVal dr As DataRow, ByRef callInfo As String) As User
      Dim retVal As New User
      Dim localInfo As String = ""
      Try
        With retVal
          .UserId = CF.NullSafeGuid(dr.Item("UserId"))
          .UserName = CF.NullSafeString(dr.Item("UserName"), "")
          .Email = CF.NullSafeString(dr.Item("Email"), "")
        End With
      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1} ", EH.GetCallerMethod(), ex.Message)
      End Try
      Return retVal
    End Function

    ''' <summary>
    ''' Returns full user name.
    ''' </summary>
    Public Shared Function GetUserFullName(ByVal usr As User, ByRef callInfo As String) As String
      Dim retVal As String = ""
      Try
        Return usr.UserName
      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1} ", EH.GetCallerMethod(), ex.Message)
        CF.SendOzzy(EH.GetCallerMethod(), ex.ToString, Nothing)
      End Try
      Return retVal
    End Function

    ''' <summary>
    ''' Returns full user name.
    ''' </summary>
    Public Shared Function GetUserFullNameByUserId(ByVal users As List(Of User), ByVal usrId As Guid, ByRef callInfo As String) As String
      Dim retVal As String = ""
      Try
        For Each U As User In users
          If U.UserId = usrId Then
            Return U.UserName
          End If
        Next
      Catch ex As Exception
        callInfo &= String.Format("{0} error: {1} ", EH.GetCallerMethod(), ex.Message)
      End Try
      Return retVal
    End Function

#End Region
     
  End Class

End Namespace
