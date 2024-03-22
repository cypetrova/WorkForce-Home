<%@ Page Title="" Language="VB" MasterPageFile="~/Site1.master" AutoEventWireup="false" CodeFile="VavejdDom2.aspx.vb" Inherits="VavejdDom2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Maincontent" Runat="Server">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
      <script >
          $.getJSON("https://api.ipify.org?format=json", function (data) {
              document.getElementById('Maincontent_myaddress').value = data.ip; 
          })

          document.getElementById('Maincontent_B1').click();

          // jquery
          $("#B1").click();
</script>
     <meta name="viewport" content="width=device-width, initial-scale=1">
    <link href="styles/vavejdDom2.css" rel="stylesheet" type="text/css" media="all" />
    <script src="Scripts/VDom2.js" type="text/javascript"></script>
    &nbsp; <asp:Label ID="potr" runat="server" Text="Label" Visible="False"></asp:Label>
            &nbsp;<asp:Label ID="Label1" runat="server" Text="Label" Visible="False"></asp:Label>
            &nbsp;<asp:Label ID="oblast" runat="server" Text="Label" Visible="False"></asp:Label>
            &nbsp;<asp:HiddenField ID="myaddress"  runat="server"></asp:HiddenField>
            <br />
    <asp:Label ID="rolia" runat="server" Text="Label" Visible="False"></asp:Label>

    <asp:Label ID="Label2" runat="server" Text="Област:" Style="width: auto" Font-Bold="True"></asp:Label>
    <asp:TextBox ID="txtArea" runat="server" Font-Bold="True" BorderStyle="None" Style="text-align: center; width: 30px;" ReadOnly="True"></asp:TextBox>
    Населено място: 
    <asp:TextBox ID="txtPlace" runat="server" BorderStyle="None" Style="text-align: center" Font-Bold="True" ReadOnly="True" Width="20px" Height="20px"></asp:TextBox>
    <asp:Label ID="Label12" runat="server" BorderStyle="None" Text="Номер на гнездото в областта" Font-Bold="True"></asp:Label>
    <asp:TextBox ID="txtAreaNest" runat="server" BorderStyle="None" Width="48px" Style="text-align: center" Font-Bold="True" ReadOnly="True"></asp:TextBox>
    <asp:Label ID="Label27" runat="server" BorderStyle="None" Text="Номер на домакинството в гнездото:" Font-Bold="True"></asp:Label>
    <asp:TextBox ID="txtHouseHoldNest" runat="server" BorderStyle="None" Width="30px" ReadOnly="True" Font-Bold="True" Style="text-align: center"></asp:TextBox>
    <asp:Label ID="Lbl2" runat="server" BorderStyle="None" Text="Дата на последния ден на наблюдавания период:" Font-Bold="True"></asp:Label>
    <asp:TextBox ID="txtDate" runat="server" Style="width: auto" BorderStyle="None" ReadOnly="True" Font-Bold="True" Text-Align="center"></asp:TextBox>
    <br />
    <br />
    Брой на членовете в домакинството
    <asp:TextBox ID="HsHldNum" runat="server" Font-Bold="True" BorderStyle="None" Style="text-align: center; " ReadOnly="True" Width="18px"></asp:TextBox>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <br />
    Ако в домакинството има лице/лица, за които не може да бъде получена информация, моля отбележете техния брой
     <asp:TextBox ID="NmbPrs" runat="server" Font-Bold="True" BorderStyle="None" Style="text-align: center; " ReadOnly="True" Width="18px"></asp:TextBox>
    &nbsp;<asp:Label ID="LMessage" runat="server" Text="Броя на лицата за които не може да бъде получена информация е по-голям от броя членове в домакинстовото" ForeColor="Red" Visible="False"></asp:Label>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />
          <br/> 
         &nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
              <div>
    <table id="tPersons"  class="w-25"
        GridLines="Both" 
        HorizontalAlign="Center" 
        Font-Names="Verdana" 
        Font-Size="8pt" 
        CellPadding="15" 
        CellSpacing="0" 
        runat="server" Width="220px"> </table>
    </div>
          &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
          <br />
          &nbsp;&nbsp;&nbsp;
          <button type="button" id="AddPrs" onclick="javascript:AddPerson();">Добави лице</button>
          &nbsp;&nbsp;&nbsp;
        <button type="button" id="ChkData" onclick="javascript:CheckData();">Проверка на данните</button>
          <br />
   &nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; 
    <p />
    <div id="MEtable-container"></div>  
    <p></p>
        <div id="SEtable-container"></div>
        <div><input type="checkbox" id="SError" name="serr"></div>
    <p />
    <table id="tPerson" border="0" class="w-50" style="width: 41%; height: auto;">
              <tr>
                <td style="width: 1609px">
                    <asp:Label ID="Label17" runat="server" Text="Д2. Пореден номер на лицето"></asp:Label>
                </td>
                <td style="width: 61px">
                    <asp:TextBox ID="PersonD2" runat="server" ReadOnly="True" Width="34px"></asp:TextBox>
                </td>
               </tr> 
              <tr>
                <td style="width: 1609px">
                    <asp:Label ID="Label11" runat="server" Text="Д4. Отношение към главата на домакинството"></asp:Label>
                    &nbsp;</td>
                   <td style="width: 45px">
                <asp:DropDownList ID="DDL4" runat="server" Style="width: 45px">
                    <asp:ListItem Text=" " Value=" "> </asp:ListItem>
                    <asp:ListItem Text="01-глава на домакинството" Value="01"></asp:ListItem>
                    <asp:ListItem Text="02-съпруг,съпруга" Value="02"></asp:ListItem>
                    <asp:ListItem Text="03-син,дъщеря" Value="03"></asp:ListItem>
                    <asp:ListItem Text="04-зет,снаха" Value="04"></asp:ListItem>
                    <asp:ListItem Text="05-внук,внучка" Value="05"></asp:ListItem>
                    <asp:ListItem Text="06-родител на главата на домакинството" Value="06"></asp:ListItem>
                    <asp:ListItem Text="07-родител на съпруга/съпругата" Value="07"></asp:ListItem>
                    <asp:ListItem Text="08-баба,дядо" Value="08"></asp:ListItem>
                    <asp:ListItem Text="09-брат,сестра на главата на домакинството " Value="09"></asp:ListItem>
                    <asp:ListItem Text="10-друго родствено лице" Value="10"></asp:ListItem>
                    <asp:ListItem Text="11-няма родствена връзка" Value="11"></asp:ListItem>
                </asp:DropDownList>
                <span id="d4_error">#</span>
                </td>
               </tr>              
              <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label14" runat="server" Text="Д5.&nbsp;Пореден номер на съпругата/ съпруга/ партньора на лицето"></asp:Label>
                  </td>
                <td style="width: 61px">           
                        <asp:DropDownList ID="DDL5" runat="server" Style="width: 45px">
                    <asp:ListItem Value=" "> </asp:ListItem>
                    <asp:ListItem>01</asp:ListItem>
                    <asp:ListItem>02</asp:ListItem>
                    <asp:ListItem>03</asp:ListItem>
                    <asp:ListItem>04</asp:ListItem>
                    <asp:ListItem>05</asp:ListItem>
                    <asp:ListItem>06</asp:ListItem>
                    <asp:ListItem>07</asp:ListItem>
                    <asp:ListItem>08</asp:ListItem>
                    <asp:ListItem>09</asp:ListItem>
                    <asp:ListItem>10</asp:ListItem>
                    <asp:ListItem>11</asp:ListItem>
                    <asp:ListItem>12</asp:ListItem>
                    <asp:ListItem>13</asp:ListItem>
                    <asp:ListItem>14</asp:ListItem>
                    <asp:ListItem>15</asp:ListItem>
                    <asp:ListItem>16</asp:ListItem>
                    <asp:ListItem>17</asp:ListItem>
                    <asp:ListItem>18</asp:ListItem>
                    <asp:ListItem>19</asp:ListItem>
                    <asp:ListItem>99</asp:ListItem>
                </asp:DropDownList>
                    <span id="d5_error">#</span> 
                </td> 
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label15" runat="server" Text="Д6. Пореден номер на бащата на лицето"></asp:Label>
                  </td>
                    <td style="width: 61px">
                <asp:DropDownList ID="DDL6" runat="server" Style="width: 45px">
                    <asp:ListItem Value=" "> </asp:ListItem>
                    <asp:ListItem>01</asp:ListItem>
                    <asp:ListItem>02</asp:ListItem>
                    <asp:ListItem>03</asp:ListItem>
                    <asp:ListItem>04</asp:ListItem>
                    <asp:ListItem>05</asp:ListItem>
                    <asp:ListItem>06</asp:ListItem>
                    <asp:ListItem>07</asp:ListItem>
                    <asp:ListItem>08</asp:ListItem>
                    <asp:ListItem>09</asp:ListItem>
                    <asp:ListItem>10</asp:ListItem>
                    <asp:ListItem>11</asp:ListItem>
                    <asp:ListItem>12</asp:ListItem>
                    <asp:ListItem>13</asp:ListItem>
                    <asp:ListItem>14</asp:ListItem>
                    <asp:ListItem>15</asp:ListItem>
                    <asp:ListItem>16</asp:ListItem>
                    <asp:ListItem>17</asp:ListItem>
                    <asp:ListItem>18</asp:ListItem>
                    <asp:ListItem>19</asp:ListItem>
                    <asp:ListItem>99</asp:ListItem>
                </asp:DropDownList>
                   <span id="d6_error">#</span> 
                </td>
               
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label16" runat="server" Text="Д7. Пореден номер на майката на лицето"></asp:Label>
                  </td>
                 <td style="width: 61px">
                   
                <asp:DropDownList ID="DDL7" runat="server" Style="width: 45px">
                    <asp:ListItem Value=" "> </asp:ListItem>
                    <asp:ListItem>01</asp:ListItem>
                    <asp:ListItem>02</asp:ListItem>
                    <asp:ListItem>03</asp:ListItem>
                    <asp:ListItem>04</asp:ListItem>
                    <asp:ListItem>05</asp:ListItem>
                    <asp:ListItem>06</asp:ListItem>
                    <asp:ListItem>07</asp:ListItem>
                    <asp:ListItem>08</asp:ListItem>
                    <asp:ListItem>09</asp:ListItem>
                    <asp:ListItem>10</asp:ListItem>
                    <asp:ListItem>11</asp:ListItem>
                    <asp:ListItem>12</asp:ListItem>
                    <asp:ListItem>13</asp:ListItem>
                    <asp:ListItem>14</asp:ListItem>
                    <asp:ListItem>15</asp:ListItem>
                    <asp:ListItem>16</asp:ListItem>
                    <asp:ListItem>17</asp:ListItem>
                    <asp:ListItem>18</asp:ListItem>
                    <asp:ListItem>19</asp:ListItem>
                    <asp:ListItem>99</asp:ListItem>
                </asp:DropDownList>
                   <span id="d7_error">#</span> 
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label21" runat="server" Text="Д8. Пол"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL8" runat="server" Width="38px">
                    <asp:ListItem Value=" ">  </asp:ListItem>
                    <asp:ListItem Value="1">1-Мъж</asp:ListItem>
                    <asp:ListItem Value="2">2-Жена</asp:ListItem>
                </asp:DropDownList>
                 <span id="d8_error">#</span>   
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label20" runat="server" Text="Д9. Ден на раждане"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:TextBox ID="TextBox7" runat="server" Style="width: 30px" onchange="javascript:calculate_age(this.id);" MaxLength="2"></asp:TextBox>
                 <span id="d9d_error">#</span>   
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label22" runat="server" Text="Д9. Месец на раждане"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:TextBox ID="TextBox8" runat="server" Style="width: 30px" onchange="javascript:calculate_age(this.id);" MaxLength="2"></asp:TextBox>
                   <span id="d9m_error">#</span> 
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label23" runat="server" Text="Д9. Година на раждане"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:TextBox ID="TextBox9" runat="server" onchange="javascript:calculate_age(this.id);" MaxLength="4" Width="47px"></asp:TextBox>
                   <span id="d9y_error">#</span> 
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px; height: 30px;">
                       <asp:Label ID="Label24" runat="server" Text="Д10. Възраст"></asp:Label>
                  </td>
                <td style="width: 61px; height: 30px;">
                    
                <asp:TextBox ID="TextBox10" name="T10" runat="server" Style="width: 45px" MaxLength="2"></asp:TextBox>
                    <span id="d10_error">#</span> 
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label25" runat="server" Text="Д11. Гражданство"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL11" runat="server" Style="width: 52px"></asp:DropDownList>
                 <span id="d11_error">#</span>   
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label26" runat="server" Text="Д12. Страна на раждане"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL12" runat="server" Style="width: 52px" onchange="javascript:D12(this.id);"></asp:DropDownList>
                  <span id="d12_error">#</span>   
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label28" runat="server" Text="Д13. Каква е причината да се заселите в България?"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL13" runat="server" Style="width: 52px" onchange="javascript:D13(this.id);">
                    <asp:ListItem Value="  "> </asp:ListItem>
                    <asp:ListItem Value="1">1-работа, намерена преди идването в България</asp:ListItem>
                    <asp:ListItem Value="2">2-търсене на работа</asp:ListItem>
                    <asp:ListItem Value="3">3-семейни причини</asp:ListItem>
                    <asp:ListItem Value="4">4-образование или обучение</asp:ListItem>
                    <asp:ListItem Value="5">5-пенсиониране, приключване с бизнеса</asp:ListItem>
                    <asp:ListItem Value="6">6-търсене на международна закрила или убежище</asp:ListItem>
                    <asp:ListItem Value="7">7-друга</asp:ListItem>
                </asp:DropDownList>
                  <span id="d13_error">#</span>
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label29" runat="server" Text="Д14. Живелите ли сте в чужбина за период от поне една година?"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL14" runat="server" Style="width: 40px" onchange="javascript:D14();">
                    <asp:ListItem Value="  "> </asp:ListItem>
                    <asp:ListItem Value="1">1-Да</asp:ListItem>
                    <asp:ListItem Value="2">2-Не</asp:ListItem>
                </asp:DropDownList>
                    <span id="d14_error">#</span> 
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label30" runat="server" Text="Д15. A) От колко години живеете в България? Б) Преди колко години се върнахте в България?"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:TextBox ID="TextBox15" runat="server" Style="width: 30px" MaxLength="2" onchange="javascript:D15();"></asp:TextBox>
                  <span id="d15_error">#</span>  
                </td>
               </tr>
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label31" runat="server" Text="Д16. Коя страна последно сте живели преди да дойдете/да се върнете в България?"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL16" runat="server" Style="width: 52px" ></asp:DropDownList>
                   <span id="d16_error">#</span>  
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label32" runat="server" Text="Д17. Страна на раждане на бащата"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL17" runat="server" Style="width: 52px" ></asp:DropDownList>
                  <span id="d17_error">#</span>   
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label33" runat="server" Text="Д18. Страна на раждане на майката"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL18" runat="server" Style="width: 52px" ></asp:DropDownList>
                  <span id="d18_error">#</span>   
                </td>
               </tr>  
       <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label34" runat="server" Text="Д19. Моля отбележете съответния код, ако лицето e:"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL19" runat="server" Width="41px" ViewStateMode="Inherit" Height="19px">
                    <asp:ListItem> </asp:ListItem>
                    <asp:ListItem Value="1">1-за всички останали лица</asp:ListItem>
                    <asp:ListItem Value="2">2-ученик, живеещ в общежитие(без студенти)</asp:ListItem>
                    <asp:ListItem Value="3">3-заминало на работа в друго населено място за период, по-голям от 6 месеца</asp:ListItem>
                </asp:DropDownList>
                   <span id="d19_error">#</span>  
                </td>
               </tr>         
         <tr>
                <td style="width: 1609px">
                       <asp:Label ID="Label35" runat="server" Text="Лицето в домакинството ли е към момента на наблюдението?"></asp:Label>
                  </td>
                <td style="width: 61px">
                    
                <asp:DropDownList ID="DDL20" runat="server" Style="width: 40px" onchange="javascript:D19();">
                    <asp:ListItem Value="  "> </asp:ListItem>
                    <asp:ListItem Value="Да">1-Да</asp:ListItem>
                    <asp:ListItem Value="Не">2-Не</asp:ListItem>
                </asp:DropDownList>
                    <span id="d20_error">#</span> 
                </td>
               </tr>  

          <tr><td></td><td><button type="button" id="bRecData" onclick="javascript:RecPerson();">Запис</button></td>
               <td></td><td><button type="button" id="bCancel" onclick="javascript:CancelPerson();">Отказ</button></td>
          </tr>
           </table>
          &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
          <br />
    <p>
        <asp:Label ID="lblМErr" Text="Задължителни проверки:" Visible="False" ForeColor="Red" runat="server"></asp:Label>
        <asp:Table ID="lstbMErr" runat="server" ForeColor="Red" Visible="False" BorderColor="Red" BorderStyle="Solid"></asp:Table>
    </p>
    <asp:Label ID="lblSErr" runat="server" ForeColor="Blue" Text="Сигнални проверки:" Visible="False"></asp:Label>
    <br />
    <p>
        <asp:Table ID="lstbSErr" runat="server" ForeColor="Blue"  BorderColor="Blue" Visible="False"  BorderStyle="Solid"></asp:Table>
    </p>
       
        <div id="d1" runat="server" > <asp:CheckBox ID="SignalChk"  Text="Така ли да остане? Изберете за да продължите напред..." ForeColor="Blue" runat="server" />   
            <br />
    </div>
    <asp:HiddenField ID="hNPers" runat="server" />
        <asp:HiddenField ID="HCheckDataPass" runat="server"></asp:HiddenField>
        <asp:HiddenField ID="HDDL13" runat="server"></asp:HiddenField>
        <asp:HiddenField ID="HTextBox15" runat="server"></asp:HiddenField>
        <asp:HiddenField ID="HDDL16" runat="server"></asp:HiddenField>
        <asp:HiddenField ID="HDDL17" runat="server"></asp:HiddenField>
        <asp:HiddenField ID="HPerson1" runat="server"   />
        <asp:HiddenField ID="HPerson2" runat="server"   />
        <asp:HiddenField ID="HPerson3" runat="server"   />
        <asp:HiddenField ID="HPerson4" runat="server"  />
        <asp:HiddenField ID="HPerson5" runat="server"   />
        <asp:HiddenField ID="HPerson6" runat="server"  />
        <asp:HiddenField ID="HPerson7" runat="server"  />
        <asp:HiddenField ID="HPerson8" runat="server"  />
        <asp:HiddenField ID="HPerson9" runat="server"  />
        <asp:HiddenField ID="HPerson10" runat="server"  />
        <asp:HiddenField ID="HPerson11" runat="server"  />
        <asp:HiddenField ID="HPerson12" runat="server"  />
        <asp:HiddenField ID="HPerson13" runat="server"  />
        <asp:HiddenField ID="HPerson14" runat="server"  />
        <asp:HiddenField ID="HPerson15" runat="server"  />
        <asp:HiddenField ID="HPerson16" runat="server"  />
        <asp:HiddenField ID="HPerson17" runat="server"  />
        <asp:HiddenField ID="HPerson18" runat="server"  />
        <asp:HiddenField ID="HPerson19" runat="server"  />
        <asp:HiddenField ID="HRowN" runat="server" />
        <asp:HiddenField ID="HObsTime" runat="server" />
    <br />
&nbsp;
         <%-- <asp:Button ID="PrevPage" Text="Предишна" runat="server" Width="93px" />
          <br /> &nbsp;
          <asp:Button ID="NextPage" Text="Следваща" runat="server" Visible="True" Width="91px" />
    <br />--%>
    <br />
    <p style="height: 27px">
        <asp:Button ID="PrevPage" runat="server" Text="Предишна" Style="margin-left: 3px;" CssClass="float-left"  />
        <asp:Button ID="NextPage" runat="server" Text="Следваща" Style="margin-left: 223px;" CssClass="float-right" Width="91px" />
    </p>

    <asp:Button ID="B1" runat="server" Text=" " style="display:none" />
</asp:Content>

