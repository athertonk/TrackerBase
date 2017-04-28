<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeFile="ProjectHome.aspx.vb"
  Inherits="ProjectHome" EnableEventValidation="false" %>

<%@ MasterType VirtualPath="~/Site.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
  <link href="/Styles/toggle-switch.css" rel="stylesheet" type="text/css" />
  <link href="/Styles/ProjectHome.css?v=20150918" rel="stylesheet" type="text/css" />
  <asp:Literal ID="paramHolder" runat="server" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
  <asp:HiddenField ID="uxHiddenProjectAddress" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectCity" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectRegion" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectRegionAbbr" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectZip" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectSubRegion" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectSubRegionCode" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectRegionLatLons" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectSubRegionLatLons" runat="server" />
  <asp:HiddenField ID="uxHiddenProjectLocation" runat="server" />

  <div id="uxContainer">
    <%-- main div to contain inline page elements --%>

    <div id="uxProjectInfoContainer" class="" title="This shows the project name and location">
      <span class="info-text" id="uxProjectLocationHeader">Location of <span class="info-text" id="uxProjectName"></span>:</span>
      <span class="info-text" id="uxProjectLocation"></span>
      <div class="right map-view-control">
        <label class="checkbox toggle candy" onclick="" title="Set to Off to see a table view of features instead of the map view.">
          <input id="uxToggleMapView" type="checkbox" onclick="ToggleMapView(this.checked, true);" checked />
          <p>
            <span>On</span>
            <span>Off</span>
          </p>
          <a class="slide-button"></a>
        </label>
      </div>
      <span class="info-text right" id="uxProjectView">Map View:</span>
    </div>
    <label runat="server" id="uxInfo"></label>

    <div class="col--mask right--menu clear">
      <%-- <div class="col--left">--%>
      <div class="col--2 home-menu">
        <div id="uxMapControls" class="">
          <div id="uxAccordionNav" class="accordion collapsible">
            <h3>Map Controls</h3>
            <div style="overflow: hidden;">
              <ul id="uxNavTools">
                <li>
                  <div class=" left  " style="width: 40%;" title="Click On to use your scroll wheel to zoom in and out on the map">
                    <label class="checkbox toggle candy" onclick="">
                      <input id="uxToggleZoomWheel" type="checkbox" checked="checked" onclick="toggleZoom(!this.checked)" />
                      <p><span>On</span><span>Off</span></p>
                      <a class="slide-button"></a>
                    </label>
                  </div>
                  <div class="valign-outer right" style="width: 55%;">
                    <div class="valign-middle">
                      <div class="valign-inner">Mouse wheel zoom</div>
                    </div>
                  </div>
                  <br />
                  <br />
                </li>
                <li>
                  <div class=" left" style="width: 40%;" title="Click to toggle visibility of administrative boundaries">
                    <label class="checkbox toggle candy" onclick="">
                      <input id="uxToggleAdminBdry" type="checkbox" onclick="showAdminBdryOverlay = !showAdminBdryOverlay; ToggleAdminBdry();" />
                      <p><span>On</span><span>Off</span></p>
                      <a class="slide-button"></a>
                    </label>
                  </div>
                  <div class="valign-outer right" style="width: 55%;">
                    <div class="valign-middle">
                      <div class="valign-inner">
                        Administrative Boundaries
                        <img src="/images/about.gif" alt="Administrative boundaries layer information."
                          title="Shows state and county boundaries."
                          onmouseover="$('[id$=uxLayerInfo]').removeClass('display-none');$('[id$=uxLayerInfo]').html('Toggles state and county boundary visibility.<br />');"
                          onmouseout="$('[id$=uxLayerInfo]').addClass('display-none');$('[id$=uxLayerInfo]').html('');" />
                      </div>
                    </div>
                  </div>
                  <br />
                  <br />
                </li>
                <li>
                  <div class=" left" style="width: 40%;" title="Click to toggle visibility of the soils layer">
                    <label class="checkbox toggle candy" onclick="">
                      <input id="uxToggleSoils" type="checkbox" onclick="showSoilsOverlay = !showSoilsOverlay; ToggleSoils();" />
                      <p><span>On</span><span>Off</span></p>
                      <a class="slide-button"></a>
                    </label>
                  </div>
                  <div class="valign-outer right" style="width: 55%;">
                    <div class="valign-middle">
                      <div class="valign-inner">
                        Soils
                        <img src="/images/about.gif" alt="Soils layer information."
                          title="Soils layer is only visible at closer zoom levels."
                          onmouseover="$('[id$=uxLayerInfo]').removeClass('display-none');$('[id$=uxLayerInfo]').html('Soils layer is only visible at closer zoom levels.<br />');"
                          onmouseout="$('[id$=uxLayerInfo]').addClass('display-none');$('[id$=uxLayerInfo]').html('');" />
                      </div>
                    </div>
                  </div>
                  <br />
                  <br />
                </li>
                <li>
                  <div class=" left" style="width: 40%;" title="Click to toggle topographic background">
                    <label class="checkbox toggle candy" onclick="">
                      <input type="checkbox" onclick="showTopo = !showTopo; ToggleTopo();" />
                      <p><span>On</span><span>Off</span></p>
                      <a class="slide-button"></a>
                    </label>
                  </div>
                  <div class="valign-outer right" style="width: 55%;">
                    <div class="valign-middle">
                      <div class="valign-inner">
                        USA Topo<img src="/images/about.gif" alt="Topo map layer information."
                          title="USA Topo map is not visible beyond a certain zoomed-in level."
                          onmouseover="$('[id$=uxLayerInfo]').removeClass('display-none');$('[id$=uxLayerInfo]').html('USA Topo map is not visible beyond a certain zoomed-in level.<br />');"
                          onmouseout="$('[id$=uxLayerInfo]').addClass('display-none');$('[id$=uxLayerInfo]').html('');" />
                      </div>
                    </div>
                  </div>
                  <br />
                  <br />
                </li>
                <li>
                  <div id="uxLayerInfo" class="display-none">
                    Hover over info button to see display.<br />
                  </div>
                </li>
                <li class=" ">
                  <input type="button" class="main-menu" id="uxZoomToAllFeatures" onclick="SetMapExtentByOids();"
                    title="Set map extent to include all features for this project" value="Zoom to all Features" />
                </li>
                <li class="display-none">
                  <input type="button" class="main-menu" id="uxOpenGeometryTools" onclick="ShowGeometryTools();"
                    title="Open tools for calculating area and distance" value="Geometry Tools" />
                </li>
              </ul>
            </div>
          </div>
          <div id="uxAccordionFeatureTools" class="accordion accord-tools">
            <h3 class="draw-stuff">Field Tools</h3>
            <div>
              <input type="button" id="uxOpenFieldTools" class="main-menu" onclick="BeginNewField(this);"
                title="Open field creation tools" value="Create New Area" />
              <p>
                Select area to use the following:
              </p>
              <input type="button" id="uxEditField" class="main-menu" data-sel-req="field" onclick="EditField(this);"
                title="Open field editing tools" value="Edit Area" />
              <input type="button" id="uxDeleteField" class="main-menu" data-sel-req="field" onclick="DeleteField(editingOid);"
                title="Delete the selected field" value="Delete Area" />
            </div>
            
          </div>
        </div>
      </div>

      <div id="uxPopupCenter" class="col--1 home-main">
        <div id="uxMapContainer" data-view="map"></div>

        <div id="uxFieldContainer" data-view="field gis"></div>
        <%-- render with templating --%>

        <script id="fieldsTmpl" type="text/x-jsrender">
          <div id="uxFieldsContainer" data-view="field gis" class="">
            <div class="clear-fix">
              <span class="text-center list-title">Field</span>
              <span class="right">
                <input type="button" value="Refresh" class="accord-button" title="Reload field from server" onclick="fieldsRetrievedIndx = -99; ReloadFields(this); return false;" />
                <input type="button" value="Close" class="accord-button" title="Return to map view" onclick="$('#uxToggleMapView').trigger('click'); return false;" />
              </span>
            </div>
            <div><span id="uxFieldsInfo">You do not have a field created. Use the tools button to create a new field.</span></div>
            {^{for fields}}
            <div id="uxFieldAccordion{{:#index}}" class="accord-group notaccordion collapsible collapsed">
              <h3 id="uxFieldsHeader{{:#index}}" class="accord-header-items">
                <input type="hidden" id="uxFieldOid{{:#index}}" value="{{:fieldRecord.ObjectID}}" />
                <input type="hidden" id="uxFieldGuid{{:#index}}" value="{{:fieldDatum.GUID}}" />
                <input type="radio" name="FieldSelect" id="uxFieldSelect{{:#index}}" class="accord-sel" />
                <span class="accord-header-separate display-none OLD"><span>Area ID: </span><span>{{:fieldRecord.FieldName}}</span>
                  {{if !fieldRecord.Shape || fieldRecord.Shape.trim().length<1}}<span class="warning">No Shape</span>{{/if}}</span>
                <span class="accord-header-separate-6">
                  <span>Field: </span><span>{{:fieldRecord.FieldName}}</span>
                  <span>Acres: </span><span class="text-right">{{positive:fieldRecord.TotalArea}}</span>
                  {{if !fieldRecord.Shape || fieldRecord.Shape.trim().length<1}}<span class="warning">No Shape</span>{{/if}}</span>
              </h3>
              <div>
                <table id="uxSelectedFieldDetails{{:#index}}" class="field-table full-width">
                  <tbody>
                    <tr>
                      <td>Acres: </td>
                      <td>
                        <label id="uxFieldTotalArea{{:#index}}">{{positive:fieldRecord.TotalArea}}{{if !fieldRecord.Shape || fieldRecord.Shape.trim().length<1}}<span class="warning">No Shape</span>{{/if}}</label></td>
                      <td></td>
                    </tr>
                    <tr>
                    </tr>
                    <tr class="clear">
                      <td class="no-align">Created:
                        <label id="uxFieldsCreated{{:#index}}">{{SDate fieldDatum.Created /}}</label></td>
                      <td class="no-align">Edited:
                        <label id="uxFieldsEdited{{:#index}}">{{SDate fieldDatum.Edited /}}</label></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
            {{/for}}
          </div>
        </script>

        <div id="uxEditFieldContainer" class="display-none popup-tools draggable field-tools">
          <script id="editFieldsTmpl" type="text/x-jsrender">
            {^{if selectedID && selectedID !== '0'}}
  <div id="uxEditFieldForm" class="popup--form">
    <div id="uxEditFieldHeader" class="popup-tools-header">
      <h3 id="uxEditFieldTitle" class="popup--tools-title">Edit Field</h3>
      <div class="popup-control-panel">
        <img title="This form is draggable when you see the arrows" alt="Draggable form" src="/images/move.png"
          class="control-drag" />
        <img title="Close this form" alt="Close form" src="/images/close.png"
          id="uxEditFieldDrawCancel" data-form-cancel="field-tools" class="control-close" onclick="CancelFieldDraw();" />
      </div>
    </div>
    <hr />
    <div id="uxEditFieldMain" class="input-small popup-tools-main field-tools-main">
      <label id="uxEditFieldInfo" class="info-text">You may change any of the following attributes. Click Edit Shape to edit the geometry.</label>
      <ul>
        <li>
          <label id="uxEditFieldFieldNameInfo" class="left-column">ID (required):</label>
          <input id="uxEditFieldFieldName" type="text" data-type="text" maxlength="15" class="right-side"
            onchange="TrimStart(this);ImposeMaxLength(this, 15);" onblur="TrimInput(this);this.onchange();" onkeyup="this.onchange();"
            onkeypress="this.onchange();" />
          <span class="text-small">&nbsp;<span id="uxEditFieldFieldNameCount">15</span><span> characters remaining</span></span>
        </li>
        <li id="uxHoverInfo" class="display-none">
          <label id="uxHoverInfoRight" class=" "></label>
        </li>
        <li>
          <label id="uxEditFieldNotesInfo" class="left-column">
            Notes:
          <br />
            <span class="text-small"><span id="uxEditFieldNotesCount">100</span><span> characters remaining</span></span></label>
          <textarea id="uxEditFieldNotes" class="right-side" name="text" cols="34" rows="3"
            onchange="TrimStart(this);ImposeMaxLength(this, 100);" onblur="TrimInput(this);this.onchange();" onkeyup="this.onchange();"
            onkeypress="this.onchange();"></textarea>
          <br />
        </li>
      </ul>
      <div id="uxEditFieldAccordionAttrs" class="accord-group notaccordion collapsible collapsed clear display-none">
        <h3 id="uxEditFieldsHeaderAttrs" class="accord-header-items">
          <span>More attributes </span>
        </h3>
        <div class="tan">
          <ul>
            <li>
              <label id="uxEditFieldWatershedCodeInfo" class="left-column">12-digit Watershed:</label>
              <input id="uxEditFieldWatershedCode" type="text" data-type="text" maxlength="12" class="right-side"
                onblur="ExtractNumber(this,0,false);" onkeyup="ExtractNumber(this,0,false);" onkeypress="return BlockNonNumbers(event, this, false, false);" />
            </li>
            <li>
              <label id="uxEditFieldFsaFarmNumInfo" class="left-column">FSA Farm number:</label>
              <input id="uxEditFieldFsaFarmNum" type="text" data-type="number" maxlength="5" class="right-side"
                onblur="ExtractNumber(this,0,false);" onkeyup="ExtractNumber(this,0,false);" onkeypress="return BlockNonNumbers(event, this, false, false);" />
            </li>
            <li>
              <label id="uxEditFieldFsaTractNumInfo" class="left-column">FSA Tract number:</label>
              <input id="uxEditFieldFsaTractNum" type="text" data-type="number" maxlength="10" class="right-side"
                onblur="ExtractNumber(this,0,false);" onkeyup="ExtractNumber(this,0,false);" onkeypress="return BlockNonNumbers(event, this, false, false);" />
            </li>
            <li>
              <label id="uxEditFieldFsaFieldNumInfo" class="left-column">FSA Field number:</label>
              <input id="uxEditFieldFsaFieldNum" type="text" data-type="number" maxlength="4" class="right-side"
                onblur="ExtractNumber(this,0,false);" onkeyup="ExtractNumber(this,0,false);" onkeypress="return BlockNonNumbers(event, this, false, false);" />
            </li>
          </ul>
        </div>
      </div>
    </div>
    <div id="uxEditFieldToolsButtonsContainer" class="field-tools-buttons-container center">
      <table id="uxEditFieldToolsButtons" class="full-width">
        <tbody>
          <tr>
            <td id="uxEditFieldToolsButtonsLeft">
              <input type="button" id="uxEditFieldDrawStart" value="Edit Shape" class="margin-small-hori"
                onclick="if (StartDrawing(this)) { GoToMap(); }" title="Edit the field's geometry" data-form-button="start-drawing" />
              <input type="button" id="uxEditFieldDrawDeleteLast" value="Delete Last Pt" class="visibility-none margin-small-hori"
                onclick="DeleteLastDrawnPoint();" title="Delete the last drawn point" data-form-button="del-last-pt" />
              <input type="button" id="uxEditFieldDrawDeleteAll" value="Delete All Pts" class="visibility-none margin-small-hori"
                onclick="DeleteAllDrawnPoints();" title="Delete all points shown" data-form-button="del-all-pts" />
              <input type="button" id="uxEditFieldDrawSubmit" value="Submit" class="margin-small-hori"
                onclick="SubmitFeature();" title="Submit the edited field" data-form-button="submit" />
            </td>
            <td id="uxEditFieldToolsButtonsRight"></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
            {{/if}}
          </script>
        </div>

        <script type="text/javascript" id="uxFieldsTemplateScript">
          var fieldsTmpl = $.templates("#fieldsTmpl");
          var editFieldsTmpl = $.templates("#editFieldsTmpl");
        </script>

      </div>
      <%--uxPopupCenter--%>
      <%-- </div>--%>
    </div>

    <div id="uxDebuggerContainer" class="display-none">
      <span id="ResultId"></span>
      <div>
        <span>Page load debugger</span>
        <asp:TextBox ID="PageLoadDebuggerInfo" runat="server" TextMode="MultiLine" Width="100%" Height="400px" Visible="true"></asp:TextBox>
      </div>
    </div>

  </div>
  <%-- END "uxContainer": main div to contain inline page elements --%>

  <%-- BEGIN: popup divs area --%>

  <div id="uxGeometryToolsContainer" class="display-none popup-tools draggable">
    <div id="uxGeometryTools">
      <div id="uxGeometryToolsHeader" class="popup-tools-header" title="Drag title area to move form">
        <span id="uxGeometryToolsTitle" class="popup-tools-title">Map Navigation Tools</span>
        <div class="popup-control-panel">
          <img title="This form is draggable when you see the arrows" alt="Draggable form" src="/images/move.png"
            class="control-drag" />
          <img title="Close this form" alt="Close form" src="/images/close.png"
            id="uxGeometryToolsClose" class="control-close" onclick="CancelGeometryTools();" />
        </div>
      </div>
      <hr />
      <div id="uxGeometryToolsMain" class="popup-tools-main">
        <input type="button" class="zoomButton" id="Button2" value="Toggle Soils Layer"
          onclick="showSoilsOverlay = !showSoilsOverlay; ToggleSoils();" runat="server" title="Click to toggle visibility of the soils layer" />
        <div>
          <input type="button" id="Button3" value="Zoom to All Features"
            onclick="SetMapExtentByOids();" title="Click to zoom map to all features for this project" />
          <span class=""></span>
        </div>
      </div>
      <div id="uxGeometryToolsButtonsContainer" class="popup-tools-buttons-container">
      </div>
    </div>
  </div>

  <div id="uxCreateFieldContainer" class="display-none popup-tools draggable field-tools">
    <div id="uxCreateFieldForm" class="popup--form">
      <div id="uxCreateFieldHeader" class="popup-tools-header">
        <h3 id="uxCreateFieldTitle" class="popup--tools-title">Create Field</h3>
        <div class="popup-control-panel">
          <img title="This form is draggable when you see the arrows" alt="Draggable form" src="/images/move.png"
            class="control-drag" />
          <img title="Close this form" alt="Close form" src="/images/close.png"
            id="uxFieldDrawCancel" data-form-cancel="field-tools" class="control-close" onclick="CancelFieldDraw();" />
        </div>
      </div>
      <hr />
      <div id="uxCreateFieldMain" class="input-small popup-tools-main field-tools-main display-none">
        <label id="uxCreateFieldInfo" class="info-text">Click on the map to add points to your field.</label>
      </div>
      <div id="uxCreateFieldButtonsContainer" class="field-tools-buttons-container center">
        <table id="uxCreateFieldButtons" class="full-width">
          <tbody>
            <tr>
              <td id="uxCreateFieldButtonsLeft">
                <input type="button" id="uxFieldDrawStart" value="Start Drawing" class="margin-small-hori"
                  onclick="if (StartDrawing(this)) { GoToMap(); }" title="Start drawing a new field" data-form-button="start-drawing" />
                <input type="button" id="uxFieldDrawDeleteLast" value="Delete Last Pt" class="visibility-none margin-small-hori"
                  onclick="DeleteLastDrawnPoint();" title="Delete the last drawn point" data-form-button="del-last-pt" />
                <input type="button" id="uxFieldDrawDeleteAll" value="Delete All Pts" class="visibility-none margin-small-hori"
                  onclick="DeleteAllDrawnPoints();" title="Delete all points shown" data-form-button="del-all-pts" />
                <input type="button" id="uxFieldDrawSubmit" value="Submit" class="visibility-none margin-small-hori"
                  onclick="SubmitFeature();" title="Submit the newly drawn field" data-form-button="submit" />
              </td>
              <td id="uxCreateFieldButtonsRight"></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  
  <%-- END: popup divs area --%>

  <%-- END: non-inline elements --%>

  <script type="text/javascript"> window.onload = function () { initialize(); } </script>
  <script type="text/javascript" src="http://maps.googleapis.com/maps/api/js?v=3&libraries=geometry"></script>
  <script type="text/javascript" src="http://serverapi.arcgisonline.com/jsapi/gmaps/?v=1.6"></script>
  <script type="text/javascript" src="https://cdn.rawgit.com/googlemaps/v3-utility-library/master/infobox/src/infobox_packed.js"></script>
  <script type="text/javascript" src="https://cdn.rawgit.com/printercu/google-maps-utility-library-v3-read-only/master/arcgislink/src/arcgislink_compiled.js"></script>
  <script type="text/javascript" src="/Scripts/ProjectHome.js?v=20150918"></script>
  <script type="text/javascript" src="/Scripts/Field.js?v=20150918"></script>
  <script type="text/javascript" src="/Scripts/UploadGIS.js?v=20150918"></script>
  <script type="text/javascript" src="/Scripts/polysnapper.js?v=20160314"></script>
</asp:Content>
