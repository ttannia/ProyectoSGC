<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TesisSGC._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="aspnetTitle" class="mb-4">Bienvenido</h1>

            <div class="d-flex flex-column gap-3" style="max-width: 250px;">
        <a href="/Socios/Index" class="btn btn-success btn-sm">Ir a Socios</a>
        <a href="/Cuentas/Index" class="btn btn-success btn-sm">Ir a Cuentas</a>
        <a href="/CuotaMensuals/Index" class="btn btn-success btn-sm">Ir a Cuotas Mensuales</a>
        <a href="/PagoSocios/Index" class="btn btn-success btn-sm">Ir a Pagos</a>


            </div>
        </section>
    </main>

</asp:Content>

