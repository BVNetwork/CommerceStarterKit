<%@ Control Language="C#" AutoEventWireup="true"
    CodeBehind="StatusMessage.ascx.cs" EnableViewState="false"
    Inherits="BVNetwork.EPiSendMail.Plugin.StatusMessage" %>

<asp:PlaceHolder ID="pnlStatusContainer" runat="server">
    <asp:Panel runat="server" Visible="False" ID="pnlErrorMessage">

        <div class="alert alert-dismissable alert-danger">
            <button type="button" class="close" data-dismiss="alert">×</button>
            <asp:Label runat="server" Font-Bold="True" ID="lblErrorMessage" />
        </div>
    </asp:Panel>
    <asp:Panel runat="server" Visible="False" ID="pnlInfoMessage">
        <div class="alert alert-dismissable alert-success">
            <button type="button" class="close" data-dismiss="alert">×</button>
            <asp:Label runat="server" ID="lblInfoMessage" />
        </div>
    </asp:Panel>
</asp:PlaceHolder>
