<%@ Page Language="C#" MasterPageFile="~/4guys.master" %>
<script runat="server">
	private void Page_Load()
	{
		Login1.Focus();
	}
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="c" Runat="Server">
	<asp:Login ID="Login1" runat="server" BackColor="#FFFFCC" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="10pt" >
		<TitleTextStyle BackColor="#4444AA" Font-Bold="True" ForeColor="#FFFFFF" />
	</asp:Login>
	<h2>
	    <p>You can log into the system as an Administrator use the username and password "admin" and "admin".</p>
        <p>To login as Managers, use the username and password: "management" and "management".</p>
        <p>To login as Personnel, use the username and password: "personnel" and "personnel".</p>
        <p>To login as Sales, use the username and password: "sales" and "sales".</p>
        <p>To login as Marketing, use the username and password: "marketing" and "marketing".</p>
        <p>To login as Purchasing, use the username and password: "purchasing" and "purchasing".</p>
        <p>To login as IT, use the username and password: "it" and "it".</p>
	</h2>
</asp:Content>


