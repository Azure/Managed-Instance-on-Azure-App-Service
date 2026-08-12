<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductMaster.aspx.cs" Inherits="devShop.ProductMaster" MasterPageFile="~/Site1.Master" EnableEventValidation="false" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="errorPopup" style="display:none; border: 1px solid red; padding: 10px; background-color: #ffe0e0;">
    <p id="errorMessageContent"></p>
    <button onclick="document.getElementById('errorPopup').style.display='none';">Close</button>
</div>
           <div class="container my-5">
<h2 class="text-center mb-4">All Category Share (%)</h2>

        </div>
        <div style="text-align: center;">
             <asp:Image ID="imgPieChart" runat="server" Height="500" Width="500" CssClass="img" />
 <br />
 <br />

        </div>
    </asp:content>
   
