<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ProductsList.aspx.cs" Inherits="devShop.ProductsList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

     <div class="container my-5">
<h2 class="text-center mb-4">Trending Products</h2>
<div id="productdiv" class="row row-cols-1 row-cols-md-2 row-cols-lg-4 g-4" runat="server">

</div>
        </div>
</asp:Content>
