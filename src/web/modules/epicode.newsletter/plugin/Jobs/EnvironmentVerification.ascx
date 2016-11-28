<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EnvironmentVerification.ascx.cs"
    Inherits="BVNetwork.EPiSendMail.Plugin.EnvironmentVerification" %>

<asp:repeater id="rptEnvVerification" runat="server" enableviewstate="false">
    <HeaderTemplate>
        <div style="background-color: #FFCCD9; border: solid 1px #FF144F; padding: 0.5em;">
            <h2 style="margin-top: 0;">Environment Verification</h2>
    </HeaderTemplate>
    <ItemTemplate>
        <div style="color: <%# ((BVNetwork.EPiSendMail.Library.VerificationType)DataBinder.Eval(Container.DataItem, "VerificationType")) == BVNetwork.EPiSendMail.Library.VerificationType.Error ? "red" : "yellow" %>; font-weight: bold;">
            <%# Eval("VerificationType") %>
        </div>
        <div>
            <%# Eval("Message") %>
        </div>
    </ItemTemplate>
    <FooterTemplate>
        </div>
    </FooterTemplate>
</asp:repeater>
