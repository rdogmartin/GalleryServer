<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="eventlog.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.eventlog" %>
<%@ Import Namespace="GalleryServer.Web" %>

<div class="gsp_content">
  <p class="gsp_a_ap_to">
    <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
      EnableViewState="false" />&nbsp;<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_User_Is_Admin_For_Label %>" />
  </p>
  <asp:PlaceHolder ID="phAdminHeader" runat="server" />
  <div class="gsp_addleftpadding5">
    <p>
      <asp:Button ID="btnClearLog" runat="server" Text="<%$ Resources:GalleryServer, Admin_Error_ClearLog_Lbl %>"
        OnClick="btnClearLog_Click" />
    </p>

    <asp:Repeater ID="rptr" runat="server" ItemType="GalleryServer.Business.Interfaces.IEvent" EnableViewState="false">
      <HeaderTemplate>
        <table class="gsp_el_tbl">
          <thead>
            <tr>
              <th></th>
              <th>Type</th>
              <th>Timestamp (UTC)</th>
              <th class="gsp_el_th_smy">Summary</th>
            </tr>
          </thead>
          <tbody>
      </HeaderTemplate>
      <ItemTemplate>
        <tr data-id="<%# Item.EventId %>">
          <td><a href="#" class="gsp_el_dtl_btn gsp_ui_icon_right_arrow gsp_hoverLink" title="Show event details"></a>
            <a href="#" class="gsp_el_d_btn gsp_hoverLink" title="Delete">
              <img src="<%= Utils.GetSkinnedUrl("/images/delete-red-s.png") %>" alt="Delete" />
            </a></td>
          <td class="gsp_el_et"><%# Item.EventType %></td>
          <td class="gsp_el_ts"><%# Item.TimestampUtc %></td>
          <td class="gsp_el_msg"><%# Item.Message %></td>
        </tr>
      </ItemTemplate>
      <FooterTemplate>
        </tbody>
        </table>
      </FooterTemplate>
    </asp:Repeater>
  </div>
  <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>

<asp:PlaceHolder runat="server">
    <script>
      (function ($) {
        $(document).ready(function() {
          
          var checkForEmptyLog = function() {
            // Show message if event log is empty
            if ($('.gsp_el_tbl tr').length <= 1) {
              $('.gsp_el_tbl').after('<p>The event log is empty.</p>');
            }
          };

          // Wire up show/collapse functionality
          $(".gsp_el_dtl_btn", $('#<%= cid %>')).click(function(e) {
            var el = $(this);
            var elRow = $(this).closest('tr');
            
            el.toggleClass('gsp_ui_icon_right_arrow gsp_ui_icon_down_arrow');
            
            if (el.hasClass('gsp_ui_icon_down_arrow')) {
              // Show event detail
                $('.gsp_el_et', elRow).addClass('gsp_wait_center');
              $.get(Gs.Vars.AppRoot + '/api/events/' + elRow.data('id'), function(html) {
                elRow.after('<tr class="gsp_el_dtl"><td colspan=4>' + html + '</td></tr>');
                $('.gsp_el_et', elRow).removeClass('gsp_wait_center');
              });
              el.attr('title', 'Collapse event details');
            } else {
              // Hide event detail
              elRow.next().remove();
              el.attr('title', 'Show event details');
            }
            return false;
          });
          
          // Wire up delete item functionality
          $(".gsp_el_d_btn", $('#<%= cid %>')).click(function(e) {
            var elRow = $(this).closest('tr');
            $('.gsp_el_et', elRow).addClass('gsp_wait_center');
            
            $.ajax(({
              type: "DELETE",
              url: Gs.Vars.AppRoot + '/api/events/' + elRow.data('id'),
              success: function (o) {
                elRow.next('.gsp_el_dtl').addBack().remove();
                checkForEmptyLog();
              },
              error: function (response) {
                Gs.Msg.show("Cannot Delete", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
              }
            }));
            return false;
          });

          checkForEmptyLog();
        });
      })(jQuery);
    </script>
</asp:PlaceHolder>
