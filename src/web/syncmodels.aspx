<%@ Page Language="c#" AutoEventWireup="false" Inherits="System.Web.UI.Page" %>
<%@ Import namespace="EPiServer.DataAbstraction" %>
<%@ Import namespace="EPiServer.ServiceLocation" %>
<script runat="server" type="text/C#">
    int _count = 0;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        Server.ScriptTimeout = 3600;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    protected void Sync(object sender, EventArgs e){
        var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
        var propertyDefinitionRepository = ServiceLocator.Current.GetInstance<IPropertyDefinitionRepository>();
        var myPages = contentTypeRepository.List();
        foreach (var myPageType in myPages)
        {
            var writableContentType = myPageType.CreateWritableClone() as ContentType;
            writableContentType.ResetContentType();
            contentTypeRepository.Save(writableContentType);

            foreach (var prop in myPageType.PropertyDefinitions)
            {
                var writable = prop.CreateWritableClone();
                writable.ResetPropertyDefinition();
                propertyDefinitionRepository.Save(writable);
            }
        }
        lblInfo.Text = "Sync done";
    }


</script>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
    <head>
        <meta content="noindex, nofollow" name="ROBOTS" />
    </head>
    <body>
        <form runat="server">
            <h1>Sync Content Models</h1>
            <asp:button runat="server" text="Sync" onclick="Sync" />
            <br />
            <asp:label runat="server" id="lblInfo" />

        </form>
    </body>
</html>