<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="TesisSGC.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

     <main>
     <section class="row" aria-labelledby="aspnetTitle">
         <h1 id="aspnetTitle" class="mb-4">Bienvenido</h1>

         <div class="d-flex flex-column gap-3" style="max-width: 250px;">
     <a href = "/MiPerfil/Index" class="btn btn-success btn-sm">Ir a mi cuenta</a>
     <a href = "/MiUsuario/Index" class="btn btn-success btn-sm">Editar usuario</a>


         </div>
     </section>
 </main>
</asp:Content>
