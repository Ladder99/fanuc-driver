<?xml version="1.0" encoding="Shift_JIS"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:bop2="http://www.fanuc.co.jp/develop/mmc/bop2"
                version="1.0">

<xsl:template match="root/chapter" mode="puttitle">
    <li>
      <a>
        <xsl:attribute name="href">#<xsl:value-of select="tag"/></xsl:attribute>
        <xsl:value-of select="title"/>
      </a>
    </li>
</xsl:template>




<xsl:template match="root/chapter" mode="putcontent">
    <xsl:variable name="tag_name" select="tag"/>
    <xsl:if test="$tag_name='All'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=All?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Data'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Data?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Handle'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Handle?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='History'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=History?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Laser'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Laser?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Misc'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Misc?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Motor'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Motor?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Ncdata'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Ncdata?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Pmc'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Pmc?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Position'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Position?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Profibus'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Profibus?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Program'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Program?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Punch'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Punch?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Servo'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Servo?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Toollife'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Toollife?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='ToolMng'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=ToolMng?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='UnSolic'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=UnSolic?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Wave'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Wave?');
        ]]></script>
    </xsl:if>
    <xsl:if test="$tag_name='Wire'">
        <script language="JavaScript"><![CDATA[
            parent.ftop.writeCookie('FANUC_LIST=Wire?');
        ]]></script>
    </xsl:if>
    
    <h3 class="label">
      <a>
        <xsl:attribute name="name">#<xsl:value-of select="tag"/></xsl:attribute>
        <xsl:value-of select="title"/>
      </a>
    </h3>

    <table border="1" cellpadding="5" cols="2" frame="below" rules="rows">
    <colgroup align="left" valign="top" width="30%"/>
    <colgroup align="left" valign="top" width="70%"/>
    <thead>
    <tr>
        <th>Function Name</th>
        <th>Brief description</th>
    </tr>
    </thead>

    <tbody>

    <xsl:apply-templates select="item" mode="putdata"/>

    </tbody>
    </table>

</xsl:template>


<xsl:template match="root/chapter/item" mode="putdata">
  <tr>
    <td class="func">
    <script language="JavaScript"><![CDATA[
      var connHs = (checkGroup.HSSB.value == 'true');
      var connEth = (checkGroup.ETHERNET.value == 'true');
      ]]>

    <!--
        HSSB条件
    -->
      <![CDATA[ var ghLink = false;       // HSSB
      ]]>
    <xsl:if test="h0ia[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F0iA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h0ib[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F0iB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h0id[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F0iD.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h0if[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F0iF.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h0ipd[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F0iP.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h0ipf[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F0iPF.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h15b[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F15B.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h15iab[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F15iAB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h16bc[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F16BC.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h16ia[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F16iA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h16ib[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F16iB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h16ip[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F16iP.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h16il[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F16iL.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h16iw[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F16iW.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h30ia[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F30iA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h30ib[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F30iB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h30ip[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F30iP.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h30il[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F30iL.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h30iwa[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F30iWA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="h30iwb[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_F30iWB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="hpmih[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_FPMiH.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="hpmid[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_FPMiD.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="hpmia[.='O']">
      <![CDATA[ ghLink |= (checkGroup.FANUC_FPMiA.value == 'true');
      ]]>
    </xsl:if>

    <!--
        ETHERNET条件
    -->
      <![CDATA[ var geLink = false;       // ETHERNET
      ]]>
    <xsl:if test="e0ia[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F0iA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e0ib[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F0iB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e0id[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F0iD.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e0if[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F0iF.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e0ipd[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F0iP.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e0ipf[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F0iPF.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e15b[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F15B.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e15iab[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F15iAB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e16bc[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F16BC.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e16ia[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F16iA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e16ib[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F16iB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e16ip[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F16iP.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e16il[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F16iL.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e16iw[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F16iW.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e30ia[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F30iA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e30ib[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F30iB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e30ip[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F30iP.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e30il[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F30iL.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e30iwa[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F30iWA.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="e30iwb[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_F30iWB.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="epmih[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_FPMiH.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="epmid[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_FPMiD.value == 'true');
      ]]>
    </xsl:if>
    <xsl:if test="epmia[.='O']">
      <![CDATA[ geLink |= (checkGroup.FANUC_FPMiA.value == 'true');
      ]]>
    </xsl:if>
      <![CDATA[
      if( (connHs && ghLink) || (connEth && geLink) ) {
          // １つでも条件があれば表示
          document.write(']]><a  target="_parent">
              <xsl:attribute name="href"><xsl:value-of select="fpage"/></xsl:attribute>
              <xsl:value-of select="fname"/></a><![CDATA[');
      }
      else {
          // 条件が当てはまらなけらば非表示
          document.write(']]><xsl:value-of select="fname"/><![CDATA[');
      }
      ]]>
    </script>
    </td>
    <td>
      <xsl:value-of select="explanation"/>
    </td>
  </tr>
</xsl:template>


<xsl:template match="/">

<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html; charset=Shift_JIS"/>
    <meta http-equiv="Content-Script-Type" content="text/javascript"/>
    <link rel="stylesheet" type="text/css" href="../fwlib32.css"/>
    <script language="JavaScript"><![CDATA[
        var endPrefix = "@";               // 接頭語の終了識別子(上書き禁止)
        var endResult = "?";               // 結果の終了識別子(上書き禁止)
        var typeCookie = "#FANUC_Ver2.0#"; // クッキー情報の終了識別子(上書き禁止)
        var typeDivid = "=";               // 分離識別子(上書き禁止)

        // クッキーの内容を取得する
        function setCookie(){
            var strCookieInfo = document.cookie;
            var sIndex = 0;                // 開始オフセットの位置
            var eIndex = 0;                // 終了オフセットの位置

            sIndex = strCookieInfo.indexOf(typeCookie);
            eIndex = strCookieInfo.indexOf(typeCookie , sIndex+typeCookie.length);

            if (sIndex != -1) {
                // 取得文字列
                var elemNum = checkGroup.elements.length;
                var strResult = strCookieInfo.substring(sIndex+typeCookie.length , eIndex);
                sIndex = strResult.indexOf(typeDivid,0);
                sIndex ++;
                var strTemp="";
                for(var i = 0; i < elemNum; i++) {
                    eIndex = strResult.indexOf( endResult ,  sIndex);
                    strTemp = strResult.substring(sIndex , eIndex);
                    checkGroup.elements[ i ].value = strTemp;
                    sIndex = eIndex + 1;
                }
            }
        }

        // プルダウンメニューのメニューを表示させる
        function optionCheck(){
            if(parent.ftop != null)
            {
                parent.ftop.setOption();
            }
            window.focus();
        }

        function writeLink(id){
            var val = checkGroup.elements[id].value;
            if(val =='O')
            {
                document.write(']]><xsl:value-of select="fname"/><![CDATA[');
            }
            else
            {
                document.write(']]><xsl:value-of select="fname"/><![CDATA[');
            }
        }
      ]]>
    </script>
  </head>
  <body bgcolor="#FFFFFF" onLoad="optionCheck()">
    <form name='checkGroup'>
        <input type='hidden' name='HSSB' value='true'/>
        <input type='hidden' name='ETHERNET' value='true'/>
        <input type='hidden' name='FANUC_F0iA' value='true'/>
        <input type='hidden' name='FANUC_F0iB' value='true'/>
        <input type='hidden' name='FANUC_F0iD' value='true'/>
        <input type='hidden' name='FANUC_F0iF' value='true'/>
        <input type='hidden' name='FANUC_F0iP' value='true'/>
        <input type='hidden' name='FANUC_F0iPF' value='true'/>
        <input type='hidden' name='FANUC_F15B' value='true'/>
        <input type='hidden' name='FANUC_F15iAB' value='true'/>
        <input type='hidden' name='FANUC_F16BC' value='true'/>
        <input type='hidden' name='FANUC_F16iA' value='true'/>
        <input type='hidden' name='FANUC_F16iB' value='true'/>
        <input type='hidden' name='FANUC_F16iP' value='true'/>
        <input type='hidden' name='FANUC_F16iL' value='true'/>
        <input type='hidden' name='FANUC_F16iW' value='true'/>
        <input type='hidden' name='FANUC_F30iA' value='true'/>
        <input type='hidden' name='FANUC_F30iB' value='true'/>
        <input type='hidden' name='FANUC_F30iP' value='true'/>
        <input type='hidden' name='FANUC_F30iL' value='true'/>
        <input type='hidden' name='FANUC_F30iWA' value='true'/>
        <input type='hidden' name='FANUC_F30iWB' value='true'/>
        <input type='hidden' name='FANUC_FPMiH' value='true'/>
        <input type='hidden' name='FANUC_FPMiD' value='true'/>
        <input type='hidden' name='FANUC_FPMiA' value='true'/>
    </form>
    <script language="javascript">
      <![CDATA[ setCookie();
      ]]>
    </script>
    <div class="text">
      <xsl:apply-templates select="root/chapter" mode="putcontent"/>
    </div>
  </body>
</html>

</xsl:template>

</xsl:stylesheet>
