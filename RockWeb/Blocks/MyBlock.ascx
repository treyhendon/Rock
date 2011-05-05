﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyBlock.ascx.cs" Inherits="Rock.Web.Blocks.MyBlock" %>
<script>
    $(document).ready(function () {

        //attach a jQuery live event to the button
        $('#getjson-button').live('click', function () {
            alert('calling rest');
            $.ajax({
                type: "GET",
                dataType: 'json',
                url: '/Rest.svc/person/1',
                success: function (data, status, xhr) {
                    alert(data.FirstName + ' ' + data.LastName)
                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
        });

        $('#getpagemethod-button').live('click', function () {
            alert('calling pagemethod');
            PageMethods.GetPerson(1, OnSucceeded, OnFailed);
        });

        function OnSucceeded(data, userContext, methodName) {
            alert(data.FirstName + ' ' + data.LastName);
        }

        function OnFailed(error, userContext, methodName) {
            alert(error);
        }

    });
    
</script>


<p>MY BLOCK CONTROL</p>
<%= ThemePath %>
<asp:literal ID="lPersonName" runat="server"></asp:literal><br />
<asp:literal ID="lBlockDetails" runat="server"></asp:literal><br />
<asp:literal ID="lBlockTime" runat="server"></asp:literal><br />
<asp:literal ID="lItemCache" runat="server"></asp:literal><br />
<asp:Literal ID="lItemTest" runat="server"></asp:Literal><br />
<asp:Literal ID="lParentGroups" runat="server"></asp:Literal><br />
<asp:Literal ID="lChildGroups" runat="server"></asp:Literal><br />
<asp:UpdatePanel ID="pnlAttributeValues" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlAttribute" runat="server">
            This DIV's bgcolor is from an attribute value <br />
            Best sci-fi movie: <asp:Literal ID="lMovie" runat="server"></asp:Literal>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:Button ID="bFlushItemCache" runat="server" Text="Flush Item Cache" onclick="bFlushItemCache_Click" />

<br />
<br />
<a href="#" id="getjson-button">Get JSON Data</a> (must be logged in)
<a href="#" id="getpagemethod-button">Get PageMethod Data</a> (must be logged in)
