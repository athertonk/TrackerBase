#Region "Imports"
Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.ServiceModel
Imports System.Web.Script.Serialization
Imports System.Xml
Imports System.Xml.Linq

Imports CE = CommonEnums
Imports CF = CommonFunctions
Imports CV = CommonVariables 
Imports MAP = Mapping
Imports SC = StatesCountiesEtc
Imports Tracker.Model

#End Region

Partial Class ProjectHome
  Inherits System.Web.UI.Page

#Region "Module variables"
  Dim callInfo As String = ""
  Dim usrName As String = "" 
  Dim projectName As String = ""
  Dim projectId As Integer = 0
  Dim roleName As String = ""
  Dim roleId As Integer = 0
  Dim sessionPrjId As String = CV.SessionProjectId
  Dim sessionPgFlag As String = CV.SessionPageFlag
  Dim prjRegion As String = ""
  Dim prjRegionAbbr As String = ""
  Dim prjSubRegionAbbr As String = ""

  Dim BR As String = CV.HtmlLineBreak
  Dim dataDb As String = CF.GetDataDatabaseName()
  Dim dataConn As String = CF.GetBaseDatabaseConnString()

  Dim serializer As New JavaScriptSerializer   
  Dim myFieldHelper As New FieldHelper

#End Region

  Public Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Try
      Page.Title = CF.GetPageTitle("Project Home")
      If False = HttpContext.Current.User.Identity.IsAuthenticated Then Response.Redirect("/Account/Login")

      'Need this info to get existing data
      Dim usr As User = UserHelper.GetCurrentUser(Nothing)
      If usr.UserId = Guid.Empty Then Response.Redirect("/Account/Login")
      usrName = UserHelper.GetUserFullName(usr, Nothing)

      If Page.IsPostBack Then CF.SendOzzy(usrName & " post back", Page.Form.Action, Nothing) ' ----- debug of some sort

      ' Non IE Browser?
      If (Request.Browser.MSDomVersion.Major = 0) Then Response.Cache.SetNoStore() ' No client side caching for non IE browsers

      Dim projectIdVal As String = Session(sessionPrjId)
      If String.IsNullOrWhiteSpace(projectIdVal) OrElse Not Integer.TryParse(projectIdVal, projectId) Then Response.Redirect("/ProjectMgmt")
      CType(Master.FindControl("uxHiddenProjectId"), HiddenField).Value = projectId
      CType(Master.FindControl("uxHiddenProjectName"), HiddenField).Value = CF.GetProjectNameByProjectId(projectId, Nothing)

      Dim currFlag As String = CType(Master.FindControl("uxHiddenPageFlag"), HiddenField).Value.ToLower
      If currFlag <> CE.MenuItemValues.drawstuff.ToString Then
        CType(Master.FindControl("uxHiddenPageFlag"), HiddenField).Value = CE.MenuItemValues.drawstuff.ToString
      End If

      uxHiddenProjectLocation.Value = ""
      Dim projectLocationInfo() As String = GetProjectLocationInfo(projectId) 'run first for project region
      If projectLocationInfo(0).Contains("error") Then CF.SendEmail(CV.ozzyEmail, CF.GetSiteEmail, "GetProjectLocationInfo", projectLocationInfo(0), Nothing)

      Dim LoadInfo As String = ""
      'LoadInfo &= LoadProjectInfo()
      LoadInfo &= LoadFields()
      'LoadInfo &= LoadFieldsInfo()
      paramHolder.Text = LoadInfo

    Catch ex As Exception
      uxInfo.InnerHtml &= String.Format(" {0} error: {1} ", MethodIdentifier, ex.ToString)
    End Try
  End Sub

  Protected Function LoadProjectInfo() As String
    Dim localInfo As String = ""
    Dim retVal As String = ""
    Dim html As New StringBuilder("<script  type='text/javascript' id='projectinfo'>" & Environment.NewLine)
    Try
      Dim prjMsgs As DataTable = CF.GetProjectMessages(projectId, localInfo)
      If localInfo.Contains("error") Then html.Append(localInfo & ";")
      Dim rw As DataRow
      For rwIx As Integer = 0 To prjMsgs.Rows.Count - 1
        rw = prjMsgs.Rows(rwIx)
      Next
    Catch ex As Exception
      html.Append("alert('" & (MethodIdentifier() & " error: " & ex.Message).Replace("error", "err").Replace(Environment.NewLine, "\n").Replace("'", """") & "');")
    End Try
    html.Append("</script>")
    retVal = html.ToString
    Return retVal
  End Function

  Protected Function LoadFields() As String
    Dim retVal As String = ""
    callInfo = ""
    Dim localInfo As String = ""
    Dim html As New StringBuilder("<script  type='text/javascript' id='loadfields'>" & Environment.NewLine)
    Dim fields As New FieldPackageList
    Try
      Try
        fields = myFieldHelper.GetFields(projectId, localInfo)
        If localInfo.ToLower.Contains("error") Then callInfo &= localInfo
      Catch ex As Exception
        callInfo &= String.Format("  {0} error: {1}  ", MethodIdentifier(), ex.ToString)
      End Try

      Dim fieldsJson As String
      Dim featCount As Integer = fields.fields.Count
      fieldsJson = "{""d"":{""__type"":""Mapping+ReturnFieldsStructure""," & serializer.Serialize(fields).TrimStart("{"c) & "}"
      html.Append("fieldsJson=" & fieldsJson & ";" & Environment.NewLine)

    Catch ex As Exception
      callInfo &= String.Format("{0}: {1}", MethodIdentifier(), ex.ToString)
    End Try
    html.Append("var loadfieldsstuff='" & callInfo.Replace(Environment.NewLine, "") & "';" & Environment.NewLine)
    html.Append("</script>")
    retVal = html.ToString
    Return retVal
  End Function

  ''' <summary>
  ''' Load any needed info
  ''' </summary>
  Protected Function LoadFieldsInfo() As String
    Dim localInfo As String = ""
    Dim retVal As String = ""
    Dim html As New StringBuilder("<script type='text/javascript' id='fieldsinfo'>" & Environment.NewLine)
    Dim countr As Integer = 0
    Try
    Catch ex As Exception
      uxInfo.InnerHtml &= String.Format(" {0} error: {1} ", MethodIdentifier, ex.ToString)
    End Try
    html.Append("</script>")
    retVal = html.ToString
    Return retVal
  End Function

  Protected Function GetProjectLocationInfo(ByVal thisProjectId As Integer) As String()
    Dim methodId As String = MethodIdentifier()
    Dim localInfo As String = ""
    Dim retVal() As String = New String() {"", "", "", "", "", "", ""} 'Start with empty strings
    Dim addrIdx As Integer = 0
    Dim townIdx As Integer = 1
    Dim zipIdx As Integer = 2
    Dim cntycodeIdx As Integer = 3
    Dim cntynameIdx As Integer = 4
    Dim regionabbrIdx As Integer = 5
    Dim regionIdx As Integer = 6
    Dim projectsFoundCount = 0
    Dim states As DataTable
    Dim operation As DataTable
    Dim stRec As DataRow
    Dim opRec As DataRow
    Try
      Dim cmdText As String
      Dim datumId As Integer = -1

      cmdText = String.Format(<a>SELECT Address, City, Zip, CountyCode, CountyName, State
            FROM {0}.Operation AS OP
            INNER JOIN {0}.ProjectDatum AS PD ON OP.ObjectID = PD.ObjectID
            WHERE PD.ProjectId = {1}</a>.Value, dataDb, thisProjectId)
      localInfo = ""
      operation = CF.GetDataTable(dataConn, cmdText, localInfo)
      If Not String.IsNullOrWhiteSpace(localInfo) Then CF.SendEmail(CV.ozzyEmail, CF.GetSiteEmail, methodId & " operation", localInfo, Nothing)
      If operation.Rows.Count < 1 Then Response.Redirect("/ProjectMgmt")

      cmdText = <a>SELECT TOP 1000 [OBJECTID]
            ,[STFID],[StateAbbr],[StateName]
            ,[MINX],[MAXX],[MINY],[MAXY]
            ,[MinLat],[MinLon],[MaxLat],[MaxLon]
            FROM [MapData].[MapData].[usaSTATE10WM]</a>.Value
      localInfo = ""
      states = CF.GetDataTable(CV.MapDataConnStr, cmdText, localInfo)
      If Not String.IsNullOrWhiteSpace(localInfo) Then CF.SendEmail(CV.ozzyEmail, CF.GetSiteEmail, methodId & " states", localInfo, Nothing)

      opRec = operation.Rows(0)
      retVal(regionabbrIdx) = CF.NullSafeString(opRec("State"), "")
      retVal(addrIdx) = CF.NullSafeString(opRec("Address"), "")
      retVal(townIdx) = CF.NullSafeString(opRec("City"), "")
      retVal(zipIdx) = CF.NullSafeString(opRec("Zip"), "")
      retVal(cntycodeIdx) = CF.NullSafeString(opRec("CountyCode"), "")
      retVal(cntynameIdx) = CF.NullSafeString(opRec("CountyName"), "")

      Dim stRecs = states.Select("StateAbbr = '" & retVal(regionabbrIdx) & "'")
      Dim stateFid As Integer = -1
      If stRecs.Count > 0 Then
        stRec = stRecs(0)
        stateFid = CF.NullSafeInteger(stRec("STFID"), -1)
        retVal(regionIdx) = CF.NullSafeString(stRec("StateName"), "")
      End If

      localInfo = ""
      SC.GetAllStateInfoFromSomeStateInfo(stateFid, retVal(regionabbrIdx), retVal(regionIdx), localInfo)
      If Not String.IsNullOrWhiteSpace(localInfo) Then CF.SendEmail(CV.ozzyEmail, CF.GetSiteEmail, methodId & " GetAllStateInfoFromSomeStateInfo", localInfo, Nothing)
      localInfo = ""
      SC.GetAllCountyInfoFromSomeCountyInfo(retVal(cntycodeIdx), retVal(cntynameIdx), stateFid, retVal(regionIdx), localInfo)
      If Not String.IsNullOrWhiteSpace(localInfo) Then CF.SendEmail(CV.ozzyEmail, CF.GetSiteEmail, methodId & " GetAllCountyInfoFromSomeCountyInfo", localInfo, Nothing)

      prjRegion = retVal(regionIdx)
      prjRegionAbbr = retVal(regionabbrIdx)
      prjSubRegionAbbr = retVal(cntycodeIdx)
      uxHiddenProjectAddress.Value = retVal(addrIdx).ToString.Trim
      uxHiddenProjectCity.Value = retVal(townIdx).ToString.Trim
      uxHiddenProjectZip.Value = retVal(zipIdx).ToString.Trim
      uxHiddenProjectSubRegionCode.Value = retVal(cntycodeIdx).ToString.Trim
      uxHiddenProjectSubRegion.Value = retVal(cntynameIdx).ToString.Trim
      uxHiddenProjectRegionAbbr.Value = retVal(regionabbrIdx).ToString.Trim
      uxHiddenProjectRegion.Value = retVal(regionIdx).ToString.Trim

      localInfo = ""
      If retVal(regionIdx) <> "" Then uxHiddenProjectRegionLatLons.Value = SC.GetRegionLatLons(retVal(regionIdx), localInfo)
      If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
      localInfo = ""
      If retVal(regionIdx) <> "" AndAlso retVal(cntynameIdx) <> "" Then uxHiddenProjectSubRegionLatLons.Value = SC.GetSubRegionLatLons(retVal(regionIdx), retVal(cntynameIdx), localInfo)
      If localInfo.ToLower.Contains("error") Then callInfo &= Environment.NewLine & localInfo
    Catch ex As Exception
      retVal(0) = methodId & " error: " & ex.Message
    End Try
    Return retVal
  End Function
   
#Region "Info/Messages/Errors"

  Private Sub SetInfoText(txt As String, Optional append As Boolean = False, Optional appendwithnewline As Boolean = False)
    Try
      Dim info As HtmlGenericControl = uxInfo
      If info.InnerHtml.Trim().Length <> 0 Then
        If append Then
          If appendwithnewline Then
            txt = BR & txt
          End If
          txt = info.InnerHtml + txt
        End If
      End If
      info.InnerHtml = txt.Replace(Environment.NewLine, BR)
    Catch ex As Exception
      ShowError(MethodIdentifier(), ex)
    End Try
  End Sub

  Private Sub ShowError(callingMethod As String, inEx As Exception)
    Try
      Dim errMsg As String = String.Format("{0} error: {1}", callingMethod, FormatErrorMessage(inEx)).Replace(Environment.NewLine, BR)
      SetInfoText(errMsg, True, True)
      Page.ClientScript.RegisterStartupScript(Me.[GetType](), "errormsg", "javascript:alert('" & errMsg & "');", True)
    Catch generatedExceptionName As Exception
      Response.Write("ShowError didn't work")
    End Try
  End Sub

  Private Function FormatErrorMessage(inEx As Exception) As String
    'Used for error message attributes
    Try
      Dim CurrentStack As New System.Diagnostics.StackTrace(inEx, True)
      Dim fln As Integer = CurrentStack.GetFrame(CurrentStack.GetFrames().Length - 1).GetFileLineNumber()
      Dim lnNum As String = " (line " & fln.ToString() & ")"
      ' (string)(fln != 0 ? " (line " + fln.ToString() + ")" : "");
      Return lnNum & " " & Convert.ToString(inEx.Message)
    Catch generatedExceptionName As Exception
      Return "FormatErrorMessage didn't work"
    End Try
  End Function

  Private Function MethodIdentifier() As String
    'Used for error message attributes (title)
    Try
      Return CF.FormatMethodIdentifier(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType.Name, New System.Diagnostics.StackFrame(1).GetMethod().Name)
    Catch ex As Exception
      uxInfo.InnerHtml = ex.ToString
      Return "Project Home MethodIdentifier didn't work"
    End Try
  End Function

#End Region

End Class
