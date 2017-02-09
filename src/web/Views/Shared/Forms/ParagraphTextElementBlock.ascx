<%@ import namespace="System.Web.Mvc" %>
<%@ import namespace="EPiServer.Core" %>
<%@ import namespace="EPiServer.Web.Mvc.Html" %>
<%@ import namespace="EPiServer.Forms.Core" %>
<%@ import namespace="EPiServer.Forms.Core.Models" %>
<%@ import namespace="EPiServer.Forms.Helpers" %>
<%@ import namespace="EPiServer.Forms.Implementation.Elements" %>


<div class="Form__Element FormParagraphText Form__Element--NonData" data-epiforms-element-name="@formElement.Code">
        <div name="@formElement.Code" id="@formElement.Guid" @Html.Raw(formElement.AttributesString)><%: EditViewFriendlyTitle %>)</div>
</div>