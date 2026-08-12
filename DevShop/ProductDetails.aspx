<%@ Page Language="c#" EnableViewState="false" CodeBehind="ProductDetails.aspx.cs" AutoEventWireup="false" Inherits="devShop.ProductDetailsPage" MasterPageFile="~/Site1.Master" %>

<asp:Content ContentPlaceHolderID="HeaderContent" runat="server">

<HTML>
  <HEAD>



      <title></title>

      <script>
          $(function () {
              $('#chatmodal').click(function (e) {
                  e.preventDefault();
                  var $this = $(this);
                  var horizontalPadding = 30;
                  var verticalPadding = 30;
                  $('<iframe id="externalSite" class="externalSite" src="' + this.href + '" />').dialog({
                      title: ($this.attr('title')) ? $this.attr('title') : 'Product Assistant',
                      autoOpen: true,
                      width: 800,
                      height: 500,
                      modal: true,
                      resizable: true,
                      autoResize: true,
                      overlay: {
                          opacity: 0.5,
                          background: "black"
                      }
                  }).width(800 - horizontalPadding).height(500 - verticalPadding);
              });
          });

          function change_image(image) {

              var container = document.getElementById("mainimage");

              container.src = image.src;
          }

          document.addEventListener("DOMContentLoaded", function (event) {

          });
      </script>
   
 </HEAD>
    </asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    
    
    <div class="container mt-5 mb-5">
    <div class="row d-flex justify-content-center">
        <div class="col-md-10">
            <div class="card">
                <div class="row">
                    <div class="col-md-6">
                        <div class="images p-3">
                            <div class="text-center p-4"> <asp:Image id="mainimage" width="300" height="350" runat="server" /></div>
                           
                                 <a style="visibility:hidden" id="chatmodal" href="<%= ConfigurationManager.AppSettings["ChatbotURL"] %>"><button class="btn btn-warning text-uppercase mr-2 px-4">Chat with AI Assistant!!</button>
                                            </a>
                             
                            </div>
                    </div>
                    <div class="col-md-6">
                <%--DB Populations Start--%>    
                        <div class="product p-4" id="proddiv">
                            <div class="d-flex justify-content-between align-items-center">
                                <div class="d-flex align-items-center"> 
                                    <i class="fa fa-long-arrow-left"></i> 

                                </div> <i class="fa fa-shopping-cart text-muted"></i>
                                
                            </div>
                            <div class="mt-4 mb-3"> <span class="text-uppercase text-muted brand" runat="server"><asp:Label ID="brand" runat="server"></asp:Label> </span>
                                <h5 class="text-uppercase"><asp:Label runat="server" ID="prodname"></asp:Label></h5>
                                <div class="price d-flex flex-row align-items-center">
                                    <span class="act-price">$<asp:Label ID="prodDiscPrice" runat="server"></asp:Label></span>
                                    <div class="ml-2"> <small class="dis-price">&nbsp$<asp:Label ID="prodPrice" runat="server"></asp:Label> 10% OFF</small></div>
                                    </div>
                            </div>
                            <p  class="about"><asp:Label ID="proddesc" runat="server"></asp:Label></p>
                            <div class="sizes mt-5">
                               <h6 class="text-uppercase">Size</h6>
                               <label class="radio">
                                   <input type="radio" name="size" value="S" checked />
                                   <span>S</span>
                                   <div class="stock-info text-center mt-1"><small class="text-muted"><asp:Label ID="stockS" runat="server"></asp:Label></small></div>
                               </label>
                               <label class="radio">
                                   <input type="radio" name="size" value="M" />
                                   <span>M</span>
                                   <div class="stock-info text-center mt-1"><small class="text-muted"><asp:Label ID="stockM" runat="server"></asp:Label></small></div>
                               </label>
                               <label class="radio">
                                   <input type="radio" name="size" value="L" />
                                   <span>L</span>
                                   <div class="stock-info text-center mt-1"><small class="text-muted"><asp:Label ID="stockL" runat="server"></asp:Label></small></div>
                               </label>
                               <label class="radio">
                                   <input type="radio" name="size" value="XL" />
                                   <span>XL</span>
                                   <div class="stock-info text-center mt-1"><small class="text-muted"><asp:Label ID="stockXL" runat="server"></asp:Label></small></div>
                               </label>
                               <label class="radio">
                                   <input type="radio" name="size" value="XXL" />
                                   <span>XXL</span>
                                   <div class="stock-info text-center mt-1"><small class="text-muted"><asp:Label ID="stockXXL" runat="server"></asp:Label></small></div>
                               </label>
                            </div>
                            <div class="cart mt-4 align-items-center"> <asp:HyperLink ID="atcLink" runat="server" class="btn btn-warning text-uppercase mr-2 px-4" Text="Buy"></asp:HyperLink> <i class="fa fa-heart text-muted"></i> <i class="fa fa-share-alt text-muted"></i> </div>
                        </div>
                        <%--DB Populations End--%>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
        </div>
</asp:Content>


