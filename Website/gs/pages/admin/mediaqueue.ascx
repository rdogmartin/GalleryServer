<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="mediaqueue.ascx.cs" Inherits="GalleryServer.Web.gs.pages.admin.mediaqueue" %>
<%@ Import Namespace="GalleryServer.Web" %>

<div id="an_test" style="background-color: green; display: none; width: 800px; height: 50px;"></div>
<div class="gsp_content">
    <p class="gsp_a_ap_to">
        <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
            EnableViewState="false" />&nbsp;<asp:Literal runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
    </p>
    <asp:PlaceHolder ID="phAdminHeader" runat="server" />

    <div class="gsp_addleftpadding5 gs_mq_ctr" runat="server">
        <div class="gs_mq_status_ctr"></div>
        <div class="gs_mq_ci_ctr"></div>

        <div class="gs_mq_tab_ctr gsp_addpadding2 gsp_tabContainer">
            <ul>
                <li class=""><a href="#<%= cid %>_mqTabWtg">
                    <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaQueue_QueueWaiting_Hdr %>" /></a></li>
                <li class=""><a href="#<%= cid %>_mqTabCmp">
                    <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaQueue_QueueComplete_Hdr %>" /></a></li>
            </ul>
            <section id="<%= cid %>_mqTabWtg" class="gs_mq_wtg_ctr ui-corner-all">
                <p class="gsp_textcenter"><%= LoadingLbl %>&nbsp;<img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" alt="" /></p>
            </section>
            <section id="<%= cid %>_mqTabCmp" class="gs_mq_cmp_ctr ui-corner-all">
                <p class="gsp_textcenter"><%= LoadingLbl %>&nbsp;<img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" alt="" /></p>
            </section>
        </div>
    </div>

    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>

<asp:PlaceHolder runat="server">

    <script id="tmplQueue" type="text/x-jsrender">
        <div data-link="class{:~getQueueStatusCssClass(QueueStatus)}">{^{:~getQueueStatusText(QueueStatus)}}&nbsp;<img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" data-link="class{:~getQueueStatusSpinnerCssClass(QueueStatus)}" alt="" /></div>
    </script>

    <script id="tmplCurrent" type="text/x-jsrender">
        {^{if currentMqItems.length > 0}}
            {^{for currentMqItems}}
            <table class="gs_mq_ci_tbl">
                <tbody>
                    <tr>
                        <td class='gs_mq_ci_img_ctr'>
                            <img class='gsp_thmb_img' src='{{:ThumbnailUrl}}'>
                        </td>
                        <td class='gs_mq_ci_info'>
                            <p class='gs_mq_ci_f'>{^{:OriginalFilename}}<img class="gs_mq_ci_to" src='<%= Utils.GetSkinnedUrl("images/arrow-right-open-s.png") %>' alt='right arrow' />{^{:NewFilename}}</p>
                            <p class="gs_mq_ci_tmr_rw"><span class="gs_mq_ci_tmr">{{: ~getElapsedTime()}}</span><a class="gs_mq_ci_cancel" href="#" data-id="{{:MediaQueueId}}"><%= CancelLbl %></a><img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" class="gsp_spinner" alt="" /></p>
                        </td>
                    </tr>
                </tbody>
            </table>
        <div class="gs_mq_ci_dtl_hdr gsp_optionsHdr gsp_collapsed ui-corner-top">
            <p title='<%= SiteOptionsTooltip %>'>
                <%= CurrentItemDetailsLbl %>
            </p>
        </div>
        <section class='gsp_optionsDtl ui-corner-bottom'>
            <table class="gs_mq_ci_dtl_tbl">
                <tbody>
                    <tr>
                        <td class='gs_mq_ci_dtl_r1c1'>
                            <p class="gs_mq_ci_asset"><span class="gsp_vibrant"><%= AssetLbl %>:</span> <a href='<%= Utils.GetUrl(PageId.mediaobject) %>&moid={{:MediaObjectId}}'>{^{:MediaObjectTitle}}</a> (<%= AlbumLbl %>  <a href='<%= Utils.GetUrl(PageId.album) %>&aid={{:AlbumId}}'>{^{:AlbumTitle}}</a>)</p>
                        </td>
                        <td>
                            <p class="gs_mq_ci_added"><span class="gsp_vibrant"><%= AddedLbl %>:</span> {^{:~getDateAsFormattedString(DateAdded)}}</p>
                        </td>
                    </tr>
                    <tr>
                        <td class=''>
                            <p class="gs_mq_ci_action"><span class="gsp_vibrant"><%= ActionLbl %>:</span> {^{:ConversionType}}</p>
                        </td>
                        <td>
                            <p class="gs_mq_ci_strtd"><span class="gsp_vibrant"><%= StartedLbl %>:</span> {^{:~getDateAsFormattedString(DateConversionStarted)}}</p>
                        </td>
                    </tr>
                </tbody>
            </table>
            <p class="gs_mq_dtl_hdr gsp_vibrant"><%= DetailLbl %></p>
            <section class='gs_mq_ci_stsdtl' title="<%= DetailTt %>">
                <section class='gs_mq_ci_stsdtl_inner ui-corner-bottom'>{^{:~convertLineBreaks(StatusDetail)}}</section>
            </section>
        </section>
        {{/for}}
        {{/if}}
    </script>

    <script id='tmplWaiting' type='text/x-jsrender'>
        {^{if waitingMqItems.length == 0}}
            <p>The queue is empty.</p>
        {{else}}
            <p class="gs_mq_wtg_hdr_ctr gsp_textright">
                <button class='gs_btnClearQueueItems'>
                    <asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaQueue_QueueWaiting_Clear_Text %>" /></button>&nbsp;<img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" class="gsp_spinner" alt="" />
            </p>
        <table class="gs_mq_incmp_tbl">
            <thead class="gsp_vibrant">
                <tr>
                    <td></td>
                    <td><%= AssetLbl %></td>
                    <td><%= StatusLbl %></td>
                    <td><%= ActionLbl %></td>
                    <td><%= AddedLbl %></td>
                </tr>
            </thead>
            <tbody>
                {^{for waitingMqItems}}
                    <tr data-mediaqueueid="{{:MediaQueueId}}">
                        <td><a href="#" title="<%= WtgRemoveTt %>" class="gsp_mq_wqi_d_btn gsp_hoverLink" data-id="{{:MediaQueueId}}" data-status="{{:Status}}">
                            <img src="<%= Utils.GetSkinnedUrl("images/delete-red-s.png") %>" alt="<%= CmpRemoveTt %>" /></a></td>
                        <td class="gs_mq_incmp_ma_td"><a href="<%= Utils.GetUrl(PageId.mediaobject) %>&moid={{:MediaObjectId}}" title="<%= AlbumLbl %>: {{stripHtml:AlbumTitle}}">
                            <img src="{{:ThumbnailUrl}}" style="max-width: 20px; max-height: 20px;"><span class="gs_mq_cmp_ma_td_mtitle">{{:MediaObjectTitle}}</span></a></td>
                        <td>{{:Status}}</td>
                        <td>{{:ConversionType}}</td>
                        <td>{{:~getDateAsFormattedString(DateAdded)}}</td>
                    </tr>
                {{/for}}
            </tbody>
        </table>
        {{/if}}
    
    </script>

    <script id='tmplComplete' type='text/x-jsrender'>
        {^{if completeMqItems.length == 0}}
            <p>This list is empty.</p>
        {{else}}
            <p class="gs_mq_cmp_hdr_ctr gsp_textright">
                <img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" class="gsp_spinner" alt="" />
                <button class='gs_btnClearQueueItems'>
                    &nbsp;
                    <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaQueue_QueueComplete_Clear_Text %>" /></button>
            </p>
        <table class="gs_mq_cmp_tbl">
            <thead class="gsp_vibrant">
                <tr>
                    <td></td>
                    <td class="gs_mq_cmp_cmd_td"><%= AssetLbl %></td>
                    <td class="gs_mq_cmp_ct_td"><%= ActionLbl %></td>
                    <td class="gs_mq_cmp_add_td"><%= AddedLbl %></td>
                    <td class="gs_mq_cmp_dr_td"><%= DurationLbl %></td>
                </tr>
            </thead>
            <tbody>
                {^{for completeMqItems}}
                    <tr class="gs_mq_cmp_row1" data-mediaqueueid="{{:MediaQueueId}}">
                        <td class="gs_mq_cmp_cmd_td">
                            <a href="#" class="gs_mq_cmp_dtl_hdr gsp_ui_icon_right_arrow gsp_hoverLink" title="<%= SiteOptionsTooltip %>"></a>
                            <img src="{{:~getStatusIconUrl(StatusInt)}}" class="gs_mq_cmp_status" alt="{{:~getStatusTt(StatusInt)}}" title="{{:~getStatusTt(StatusInt)}}" />
                            <a href="#" title="<%= CmpRemoveTt %>" class="gsp_mq_wqi_d_btn gsp_hoverLink" data-id="{{:MediaQueueId}}" data-status="{{:Status}}">
                                <img src='<%= Utils.GetSkinnedUrl("images/delete-red-s.png") %>' alt="<%= CmpRemoveTt %>" /></a>
                        </td>
                        <td class="gs_mq_cmp_ma_td"><a href="<%= Utils.GetUrl(PageId.mediaobject) %>&moid={{:MediaObjectId}}" title="<%= AlbumLbl %>: {{stripHtml:AlbumTitle}}">
                            <img src="{{:ThumbnailUrl}}" style="max-width: 20px; max-height: 20px;"><span class="gs_mq_cmp_ma_td_mtitle">{{:MediaObjectTitle}}</span></a></td>
                        <td class="gs_mq_cmp_ct_td">{{:ConversionType}}</td>
                        <td class="gs_mq_cmp_add_td">{{:~getDateAsFormattedString(DateAdded)}}</td>
                        <td class="gs_mq_cmp_dr_td"><span title="{{:~getDateAsFormattedString(DateConversionStarted)}} <%= ToLbl %> {{:~getDateAsFormattedString(DateConversionCompleted)}}">{{:~getDuration()}}</span></td>
                    </tr>
                <tr class="gs_mq_cmp_row2">
                    <td colspan="5">
                        <section class='gsp_optionsDtl ui-corner-bottom'>
                            <p class="gs_mq_dtl_hdr gsp_vibrant"><%= DetailLbl %></p>
                            <section class='gs_mq_cmp_stsdtl'>{^{:~convertLineBreaks(StatusDetail)}}</section>
                        </section>
                    </td>
                </tr>
                {{/for}}
            </tbody>
        </table>
        {{/if}}
    </script>

    <script>
        $(function () {
            //$.connection.hub.logging = true; // Uncomment to enabled logging to console
            var mqHub = $.connection.mediaQueueHub;
            var mqItems = {
                queue: null,
                currentMqItems: [],
                waitingMqItems: [],
                completeMqItems: []
            };
            var autoScroll = true;
            var curDtlsExpanded = false;
            var $scope = $('#<%= cid %>');
            var selTabCookieName = '<%= SelectedTabCookieName %>';
            var defaultIconUrl = '<%= Utils.GetSkinnedUrl("images/help_s.png").JsEncode() %>';
            var errorIconUrl = '<%= Utils.GetSkinnedUrl("images/warning-s.png").JsEncode() %>';
            var completeIconUrl = '<%= Utils.GetSkinnedUrl("images/green-check-xs.png").JsEncode() %>';
            var canceledIconUrl = '<%= Utils.GetSkinnedUrl("images/collapse-s.png").JsEncode() %>';
            var defaultIconTt = '<%= Resources.GalleryServer.Admin_MediaQueue_Cmp_DefaultIcon_Tt.JsEncode() %>';
            var errorIconTt = '<%= Resources.GalleryServer.Admin_MediaQueue_Cmp_ErrorIcon_Tt.JsEncode() %>';
            var completeIconTt = '<%= Resources.GalleryServer.Admin_MediaQueue_Cmp_CompleteIcon_Tt.JsEncode() %>';
            var canceledIconTt = '<%= Resources.GalleryServer.Admin_MediaQueue_Cmp_CanceledIcon_Tt.JsEncode() %>';
            var minutesLbl = '<%= Resources.GalleryServer.Admin_MediaQueue_Cmp_Dur_Min.JsEncode() %>';
            var queueStatusIdleText = '<%= Resources.GalleryServer.Admin_MediaQueue_Status_Idle_Text.JsEncode() %>';
            var queueStatusProcessingText = '<%= Resources.GalleryServer.Admin_MediaQueue_Status_Processing_Text.JsEncode() %>';
            var timer = null;
            var dateConversionStartedLocal;

            var formatMilliseconds = function (ms) {
                var seconds = parseInt((ms / 1000) % 60);
                var minutes = parseInt((ms / (1000 * 60)) % 60);
                var hours = parseInt((ms / (1000 * 60 * 60)) % 24);

                hours = (hours < 10) ? "0" + hours : hours;
                minutes = (minutes < 10) ? "0" + minutes : minutes;
                seconds = (seconds < 10) ? "0" + seconds : seconds;

                return hours + ":" + minutes + ":" + seconds;
            };

            var addOneSecondToDuration = function () {
                var $tmrCtr = $('.gs_mq_ci_tmr', $scope);
                var tme = $tmrCtr.text();
                var tmeParts = tme.split(':');
                var h = parseInt(tmeParts[0]);
                var m = parseInt(tmeParts[1]);
                var s = parseInt(tmeParts[2]);

                s += 1;
                if (s === 60) {
                    s = 0;
                    m += 1;
                    if (m === 60) {
                        m = 0;
                        h += 1;
                    }
                }

                h = (h < 10) ? "0" + h : h;
                m = (m < 10) ? "0" + m : m;
                s = (s < 10) ? "0" + s : s;

                $tmrCtr.text(h + ":" + m + ":" + s);
            };

            var getArrayIndexInWaitingQueue = function (mediaQueueId) {
                // Get the index of the item in either the waitingMqItems or completeMqItems array (it should only be in one, and it doesn't matter which)
                for (var i = 0; i < mqItems.waitingMqItems.length; i++) {
                    if (mqItems.waitingMqItems[i].MediaQueueId === mediaQueueId) return i;
                }

                return -1;
            };

            var getArrayIndexInCompletedQueue = function (mediaQueueId) {
                // Get the index of the item in either the waitingMqItems or completeMqItems array (it should only be in one, and it doesn't matter which)
                for (var i = 0; i < mqItems.completeMqItems.length; i++) {
                    if (mqItems.completeMqItems[i].MediaQueueId === mediaQueueId) return i;
                }

                return -1;
            };

            var removeFromWaitingQueue = function (mediaQueueId) {
                $.observable(mqItems.waitingMqItems).remove(getArrayIndexInWaitingQueue(mediaQueueId), 1);
            };

            var removeFromCompleteQueue = function (mediaQueueId) {
                $.observable(mqItems.completeMqItems).remove(getArrayIndexInCompletedQueue(mediaQueueId), 1);
            };

            var makeClearAllButtons = function () {
                $('.gs_btnClearQueueItems').not('ui-button').button({ icon: "gsp-ui-icon gsp-ui-icon-delete-red" });
            };

            var removeQueueItem = function (mediaQueueId) {
                // A queue item has been deleted on the server, so remove it from the array, which causes the row to disappear from the UI

                var removeQueueItemFromArray = function () {
                    // The item is only in 1 of the queues. We don't know which one, so just call both. No harm in that.
                    removeFromWaitingQueue(mediaQueueId);
                    removeFromCompleteQueue(mediaQueueId);
                    makeClearAllButtons();
                };

                // Highlight the row for the item, then remove
                $('.gs_mq_incmp_tbl tr[data-mediaQueueId=' + mediaQueueId + '],.gs_mq_cmp_tbl tr[data-mediaQueueId=' + mediaQueueId + ']', $scope)
                    .addClass('gs_highlight_flash_delete').one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', removeQueueItemFromArray);

                // When several rows are affected very quickly, the animationend event won't fire for every row, meaning removeQueueItemFromArray() won't 
                // get called. So we use a timeout to guarantee the array item still gets removed. There's no harm in calling the function more than once.
                setTimeout(removeQueueItemFromArray, 1000);
            }

            var deleteQueueItems = function (mediaQueueIds) {
                if ((mediaQueueIds == null) || (mediaQueueIds.length === 0))
                    return;

                $(".gs_mq_wtg_hdr_ctr .gsp_spinner,.gs_mq_cmp_hdr_ctr .gsp_spinner", $scope).show();
                $.ajax({
                    type: "DELETE",
                    url: Gs.Vars.AppRoot + '/api/mediaqueueitem',
                    data: JSON.stringify(mediaQueueIds),
                    contentType: "application/json; charset=utf-8"
                })
                .always(function () {
                    $(".gs_mq_wtg_hdr_ctr .gsp_spinner,.gs_mq_cmp_hdr_ctr .gsp_spinner", $scope).hide();
                })
                .fail(function () {
                    Gs.Msg.show("Cannot Save Changes", Gs.Utils.parseJqXhrMsg(jqXHR), { msgType: 'error', autoCloseDelay: 0 });
                });
            };

            var bindEventHandlers = function () {
                var $eventScope = $(".gs_mq_ctr", $scope);

                // Add pause/start function when user clicks status detail div
                $eventScope.on('click', '.gs_mq_ci_stsdtl', function () { autoScroll = !autoScroll; });

                // Bind the open/close action for the current item details
                $eventScope.on('click', '.gs_mq_ci_dtl_hdr.gsp_optionsHdr', function () {
                    curDtlsExpanded = !curDtlsExpanded;
                    $(this).toggleClass("gsp_expanded gsp_collapsed");
                    $('.gs_mq_ci_ctr .gsp_optionsDtl').slideToggle('fast');
                });

                // Bind the open/close action for the item details on the completed tab
                $(".gs_mq_cmp_ctr", $scope).on('click', '.gs_mq_cmp_dtl_hdr', function (e) {
                    var $el = $(this);

                    $el.toggleClass("gsp_ui_icon_right_arrow gsp_ui_icon_down_arrow");
                    $el.closest('.gs_mq_cmp_row1').next('.gs_mq_cmp_row2').find('.gsp_optionsDtl').slideToggle('fast');

                    return false;
                });

                // Bind the 'clear all' buttons
                $eventScope.on('click', '.gs_btnClearQueueItems', function (e) {
                    // Delete all media queue items in the selected tab
                    var ids = $('.gsp_mq_wqi_d_btn', $(e.currentTarget).closest('.gs_mq_wtg_ctr,.gs_mq_cmp_ctr')).map(function () {
                        return $(this).data("id");
                    }).get();

                    deleteQueueItems(ids);

                    return false;
                });

                // Bind the individual delete buttons
                $eventScope.on('click', '.gsp_mq_wqi_d_btn', function () {
                    deleteQueueItems(jQuery.makeArray($(this).data("id")));
                    return false;
                });

                $eventScope.on('click', '.gs_mq_ci_cancel', function (e) {
                    var $spinner = $(e.currentTarget).text('Canceling...').next('.gsp_spinner').show();

                    Gs.DataService.cancelMediaQueueItem($(this).data("id"), function () {
                        $spinner.hide();
                    }, function () {
                    }, function (jqXHR) {
                        Gs.Msg.show('Cannot Cancel Queue Item', Gs.Utils.parseJqXhrMsg(jqXHR), { msgType: 'error', autoCloseDelay: 0 });
                    });

                    return false;
                });
            };

            var configTooltips = function () {
                $('.gsp_admin_h2_txt', $scope).gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_MediaQueue_Overview_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_MediaQueue_Overview_Bdy.JsEncode() %>'
                });
            };

            var configTabs = function () {
                $(".gs_mq_tab_ctr", $scope).tabs({
                    active: (Gs.Vars.Cookies.get(selTabCookieName) || 0),
                    activate: function (e, ui) { Gs.Vars.Cookies.set(selTabCookieName, ui.newTab.index(), { expires: 365 }); }
                })
                    .show();
            };

            var refreshActiveMqItem = function (mqItem) {
                // Runs when a new queue item starts processing or a property of the current one is updated (except for when the StatusDetail property
                // is appended, which is handled in addToMediaQueueItemStatusDetail).
                if (mqItems.currentMqItems.length === 0) {
                    $.observable(mqItems.currentMqItems).insert(mqItem);
                } else {
                    $.observable(mqItems.currentMqItems).refresh([mqItem]);
                }

                // Restore state of current item details section
                if (curDtlsExpanded) {
                    $('.gs_mq_ci_dtl_hdr.gsp_optionsHdr', $scope).removeClass("gsp_collapsed").addClass("gsp_expanded");
                    $('.gs_mq_ci_ctr .gsp_optionsDtl', $scope).show();
                } else {
                    $('.gs_mq_ci_dtl_hdr.gsp_optionsHdr', $scope).removeClass("gsp_expanded").addClass("gsp_collapsed");
                    $('.gs_mq_ci_ctr .gsp_optionsDtl', $scope).hide();
                }
            };

            var init = function () {
                timer = new Gs.GsTimer(addOneSecondToDuration, 1000);

                $.views.helpers({
                    getQueueStatusCssClass: function (queueStatus) {
                        switch (queueStatus) {
                            case 1: return 'gs_mq_sts gs_mq_sts_idle';
                            case 2: return 'gs_mq_sts gs_mq_sts_proc';
                        }
                    },
                    getQueueStatusText: function (queueStatus) {
                        switch (queueStatus) {
                            case 1: return queueStatusIdleText;
                            case 2: return queueStatusProcessingText;
                        }
                    },
                    getQueueStatusSpinnerCssClass: function (queueStatus) {
                        switch (queueStatus) {
                            case 1: return 'gs_mq_sts_spnr_invis';
                            case 2: return 'gs_mq_sts_spnr_vis';
                        }
                    },
                    getDateAsFormattedString: function (dateValue) {
                        if (dateValue != null) {
                            return Globalize.format(new Date(dateValue), "MMM d h:mm:ss tt");
                        } else
                            return "";
                    },
                    getElapsedTime: function () {
                        if (dateConversionStartedLocal == null)
                            return "";

                        return formatMilliseconds(new Date() - dateConversionStartedLocal);
                    },
                    convertLineBreaks: function (text) {
                        return text.replace(/[ \t]*(\r\n|\n|\r)/g, "<br />");
                    },
                    getStatusIconUrl: function (status) {
                        switch (status) {
                            case 1: return errorIconUrl;
                            case 4: return canceledIconUrl;
                            case 5: return completeIconUrl;
                            default: return defaultIconUrl;
                        }
                    },
                    getStatusTt: function (status) {
                        switch (status) {
                            case 1: return errorIconTt;
                            case 4: return canceledIconTt;
                            case 5: return completeIconTt;
                            default: return defaultIconTt;
                        }
                    },
                    getDuration: function () {
                        return Globalize.format((this.data.DurationMs) / 60000, "n1") + ' ' + minutesLbl;
                    }
                });

                mqHub.server.getMediaQueue().done(function (mq) {
                    // Runs only once, on page load.
                    mqItems.queue = mq;
                    $.templates("#tmplQueue").link($(".gs_mq_status_ctr", $scope), mq);
                });

                mqHub.server.getCurrentMediaQueueItem().done(function (mqItem) {
                    // Runs only once, on page load. currentMqItems is an array instead of an object because $.observable().refresh works only with arrays.
                    if (mqItem != null) {
                        mqItems.currentMqItems.push(mqItem);

                        if (mqItem.StatusInt === 3) { // 3=Processing
                            // User loaded page while a media item is being processed. Use the duration from the server to figure out the local start time.
                            dateConversionStartedLocal = (new Date() - mqItem.DurationMs);

                            timer.start();
                        }
                    }

                    $.templates("#tmplCurrent").link($(".gs_mq_ci_ctr", $scope), mqItems);
                });

                mqHub.server.getWaitingMediaQueueItems().done(function (waitingMqItems) {
                    // Runs only once, on page load.
                    mqItems.waitingMqItems = waitingMqItems;
                    $.templates("#tmplWaiting").link($(".gs_mq_wtg_ctr", $scope), mqItems);

                    makeClearAllButtons();
                });

                mqHub.server.getCompleteMediaQueueItems().done(function (completeMqItems) {
                    // Runs only once, on page load.
                    mqItems.completeMqItems = completeMqItems;
                    $.templates("#tmplComplete").link($(".gs_mq_cmp_ctr", $scope), mqItems);

                    makeClearAllButtons();
                });
            };

            // Handle server notification that the queue status has changed
            mqHub.client.mediaQueueStatusChanged = function (mq) {
                $.observable(mqItems.queue).setProperty("QueueStatus", mq.QueueStatus);
                $.observable(mqItems.queue).setProperty("QueueStatusText", mq.QueueStatusText);
            }

            // Handle server notification that an item has been added to the queue
            mqHub.client.mediaQueueItemAdded = function (mqItem) {
                $.observable(mqItems.waitingMqItems).insert(mqItem);

                if (mqItems.waitingMqItems.length > 0) {
                    makeClearAllButtons();
                }

                $('.gs_mq_incmp_tbl tbody tr:last').addClass('gs_highlight_flash').one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
                    $(this).removeClass('gs_highlight_flash');
                });
            }

            // Handle server notification that current MQ item has been updated
            mqHub.client.mediaQueueItemStarted = function (mqItem) {
                dateConversionStartedLocal = new Date(); // Don't use mqItem.DateConversionStarted because the server time can be different than local time (even if in same time zone)
                refreshActiveMqItem(mqItem);
                timer.start();

                // Highlight the row for the item, then remove
                $('.gs_mq_incmp_tbl tr[data-mediaQueueId=' + mqItem.MediaQueueId + ']', $scope)
                    .addClass('gs_highlight_flash').one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
                        removeFromWaitingQueue(mqItem.MediaQueueId);
                        makeClearAllButtons();
                    });

                // When several rows are affected very quickly, the animationend event won't fire for every row, meaning removeQueueItemFromArray() won't 
                // get called. So we use a timeout to guarantee the array item still gets removed. There's no harm in calling the function more than once.
                setTimeout(function () {
                    removeFromWaitingQueue(mqItem.MediaQueueId);
                    makeClearAllButtons();
                }, 1000);
            }

            // Handle server notification that current MQ item has been updated
            mqHub.client.activeMediaQueueItemUpdated = function (mqItem) {
                refreshActiveMqItem(mqItem);
            }

            // Handle server notification that current MQ item has a new line of text to append to status detail
            mqHub.client.addToMediaQueueItemStatusDetail = function (statusDetailToAppend) {
                if (mqItems.currentMqItems.length === 0) return;

                $.observable(mqItems.currentMqItems[0]).setProperty("StatusDetail", mqItems.currentMqItems[0].StatusDetail + statusDetailToAppend + "<br/>");

                if (autoScroll) {
                    $('.gs_mq_ci_stsdtl').animate({ scrollTop: $(".gs_mq_ci_stsdtl_inner").height() }, 10, "linear");
                }
            }

            // Handle server notification that a media queue item has just finished
            mqHub.client.mediaQueueItemCompleted = function (mqItem) {
                timer.stop();

                $.observable(mqItems.currentMqItems).remove(0);
                $.observable(mqItems.completeMqItems).insert(0, mqItem);

                if (mqItems.completeMqItems.length > 0) {
                    makeClearAllButtons();
                }
                $('.gs_mq_cmp_tbl tbody tr:first').addClass('gs_highlight_flash').one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
                    $(this).removeClass('gs_highlight_flash');
                });
            }

            // Handle server notification that a media queue item has been deleted by another user
            mqHub.client.mediaQueueItemDeleted = function (mediaQueueId) {
                removeQueueItem(mediaQueueId);
            }

            $(document).ready(function () {
                // Connect to server via SignalR
                $.connection.hub.start().done(init);

                configTabs();

                bindEventHandlers();

                configTooltips();
            });
        });
    </script>
</asp:PlaceHolder>
