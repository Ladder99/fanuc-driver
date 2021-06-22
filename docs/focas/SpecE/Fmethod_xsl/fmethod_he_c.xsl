<?xml version="1.0" encoding="Shift_JIS"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:bop2="http://www.fanuc.co.jp/develop/mmc/bop2"
                version="1.0">


<xsl:template match="/">

<html>

  <head>
    <meta http-equiv="Content-Type" content="text/html; charset=Shift_JIS"/>
    <title><xsl:value-of select="root/func/title"/></title>
    <link rel="stylesheet" type="text/css" href="../fwlib32.css"></link>
  </head>

  <body bgcolor="#FFFFFF">
    <FONT face="Verdana,Arial,Helvetica,sans-serif">

    <!-- 関数名 -->
    <a name="Function Name"/>
    <xsl:apply-templates select="root/func/title"/>

    <div class="text">

      <!-- 関数宣言 -->
      <a name="Declaration"/>
      <xsl:apply-templates select="root/func/declare"/>

      <!-- 解説 -->
      <a name="Description"/>
      <xsl:apply-templates select="root/func/doc"/>

      <!-- 引数 -->
      <a name="Arguments"/>
      <xsl:apply-templates select="root/func/argument"/>

      <!-- 戻り値 -->
      <a name="Return"/>
      <xsl:apply-templates select="root/func/errcode"/>

      <!-- CNCオプション -->
      <a name="CNC option"/>
      <xsl:apply-templates select="root/func/option"/>

      <!-- CNCパラメータ -->
      <a name="CNC parameter"/>
      <xsl:apply-templates select="root/func/parameter"/>

      <!-- CNCモード -->
      <a name="CNC mode"/>
      <xsl:apply-templates select="root/func/mode"/>

      <!-- 対応機種 -->
      <a name="Available CNC"/>
      <xsl:apply-templates select="root/func/support"/>

      <!-- 参照 -->
      <a name="Reference"/>
      <xsl:apply-templates select="root/func/reference"/>

      <!-- プログラム例 -->
      <a name="Example"/>
      <xsl:apply-templates select="root/func/example"/>

    </div>

   </FONT>

    <hr/>

  </body>

</html>

</xsl:template>





<!-- 関数名のテンプレート  -->
<xsl:template match="root/func/title">
  <p class="ftitle"><xsl:value-of select="."/></p>
</xsl:template>





<!-- 関数宣言のテンプレート  -->
<xsl:template match="root/func/declare">
  <p class="label">Declaration</p>
  <div class="mth">
    <xsl:apply-templates select=".//vc" />
  </div>
</xsl:template>


<!-- Visual C++ 用 関数宣言のテンプレート  -->
<xsl:template match="vc">
    <xsl:choose>
        <xsl:when test="/root/func/support/m/fs0id[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/m/fs0if[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/m/fs30ia[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/m/fs30ib[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/t/fs0id[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/t/fs0if[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/t/fs30ia[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/t/fs30ib[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/p/fs0id[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/p/fs0if[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/p/fs30ib[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/l/fs30ib[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/w/fs30ia[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/w/fs30ib[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:when test="/root/func/support/pw/a[.!='X']">
           <xsl:value-of select="include"/> or "fwlib64.h"<p/>
        </xsl:when>

        <xsl:otherwise>
           <xsl:value-of select="include"/><p/>
        </xsl:otherwise>
    </xsl:choose>

  <xsl:copy-of select="prottype"/>
</xsl:template>


<!-- Visual Basic 用 関数宣言のテンプレート  -->
<xsl:template match="vb">
</xsl:template>





<!-- 解説のテンプレート  -->
<xsl:template match="root/func/doc">
  <xsl:if test="count(cmn) != 0">
    <p class="label">Description</p>
    <xsl:copy-of select="cmn"/>
  </xsl:if>
</xsl:template>





<!-- 引数のテンプレート  -->
<xsl:template match="root/func/argument">

  <!-- HSSB と Ethernet 接続共通の引数 -->
  <xsl:if test="count(common/*) != 0">
    <p class="label">Arguments</p>
    <xsl:apply-templates select="common/cmn"/>
    <xsl:apply-templates select="common/vc"/>
    <xsl:apply-templates select="common/vb"/>
  </xsl:if>

  <!-- HSSB専用 引数  -->
  <xsl:if test="count(hssb/*) != 0">
    <p class="label">Arguments : HSSB</p>
    <xsl:apply-templates select="hssb/cmn"/>
    <xsl:apply-templates select="hssb/vc"/>
    <xsl:apply-templates select="hssb/vb"/>
  </xsl:if>

  <!-- Ethernet専用 引数  -->
  <xsl:if test="count(ethr/*) != 0">
    <p class="label">Arguments : Ethernet</p>
    <xsl:apply-templates select="ethr/cmn"/>
    <xsl:apply-templates select="ethr/vc"/>
    <xsl:apply-templates select="ethr/vb"/>
  </xsl:if>

</xsl:template>


<!-- Visual C++ と Visual Basic 共通の引数のテンプレート  -->
<xsl:template match="root/func/argument/*/cmn">
  <xsl:apply-templates select="item"/>
  <xsl:copy-of select="add"/>
</xsl:template>


<!-- Visual C++ 用 引数のテンプレート  -->
<xsl:template match="root/func/argument/*/vc">
  <xsl:apply-templates select="item"/>
  <xsl:copy-of select="add"/>
</xsl:template>


<!-- Visual Basic 用 引数のテンプレート  -->
<xsl:template match="root/func/argument/*/vb">
</xsl:template>


<!-- 引数の追加  -->
<xsl:template match="root/func/argument/*/*/item">
  <div class="dtarg">
    <xsl:copy-of select="name"/>
    <xsl:if test="count(type) != 0">
      &#160;&#160;[&#160;<xsl:copy-of select="type"/>&#160;]
    </xsl:if>
  </div>
  <div class="ddarg">
    <xsl:copy-of select="content"/>
  </div>
</xsl:template>





<!-- 戻り値のテンプレート -->
<xsl:template match="root/func/errcode">
  <p class="label">Return</p>
  <p>EW_OK is returned on successful completion, otherwise any value except EW_OK is returned.</p>
  <!-- 成功するとEW_OKを返し、エラーの場合はEW_OK以外の値を返します。-->
  <xsl:if test="count(item) != 0">
    <p>The major error codes are as follows.</p>
    <!-- 主なエラーは以下の通りです。-->
    <table border="1">
      <colgroup align="center" valign="top"/>
      <colgroup align="left" valign="top"/>
      <colgroup align="left" valign="top"/>
      <tr>
        <th>Return code</th>
        <th>Meaning/Error handling</th>
      </tr>
      <xsl:apply-templates select="item"/>
    </table>
  </xsl:if>

  <p><xsl:copy-of select="add"/></p>

  <p>
    <xsl:if test="count(item) != 0">
      As for the other return codes or the details, see 
      <!-- その他の戻り値やエラーの詳細については、-->
    </xsl:if>
    <xsl:if test="count(item) = 0">
      As for the details, see 
      <!-- エラーの詳細については、-->
    </xsl:if>
    "<a href="errcode.htm" target="_parent">Return status of Data window function</a>"
    <!-- 「データウィンドウ関数の戻り値」を参照して下さい。-->
  </p>
</xsl:template>

<!-- エラーコード -->
<xsl:template match="item">
  <tr>
<!--<td><xsl:copy-of select="name"/></td>-->
    <xsl:choose>
        <xsl:when test="name[.='EW_PROTOCOL']">
           <td><xsl:copy-of select="name"/><br/>(-17)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_SOCKET']">
           <td><xsl:copy-of select="name"/><br/>(-16)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_NODLL']">
           <td><xsl:copy-of select="name"/><br/>(-15)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_BUS']">
           <td><xsl:copy-of select="name"/><br/>(-11)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_SYSTEM2']">
           <td><xsl:copy-of select="name"/><br/>(-10)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_HSSB']">
           <td><xsl:copy-of select="name"/><br/>(-9)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_HANDLE']">
           <td><xsl:copy-of select="name"/><br/>(-8)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_VERSION']">
           <td><xsl:copy-of select="name"/><br/>(-7)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_UNEXP']">
           <td><xsl:copy-of select="name"/><br/>(-6)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_SYSTEM']">
           <td><xsl:copy-of select="name"/><br/>(-5)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_PARITY']">
           <td><xsl:copy-of select="name"/><br/>(-4)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_MMCSYS']">
           <td><xsl:copy-of select="name"/><br/>(-3)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_RESET']">
           <td><xsl:copy-of select="name"/><br/>(-2)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_BUSY']">
           <td><xsl:copy-of select="name"/><br/>(-1)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_OK']">
           <td><xsl:copy-of select="name"/><br/>(0)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_FUNC']">
           <td><xsl:copy-of select="name"/><br/>(1)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_NOPMC']">
           <td><xsl:copy-of select="name"/><br/>(1)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_LENGTH']">
           <td><xsl:copy-of select="name"/><br/>(2)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_NUMBER']">
           <td><xsl:copy-of select="name"/><br/>(3)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_RANGE']">
           <td><xsl:copy-of select="name"/><br/>(3)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_ATTRIB']">
           <td><xsl:copy-of select="name"/><br/>(4)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_TYPE']">
           <td><xsl:copy-of select="name"/><br/>(4)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_DATA']">
           <td><xsl:copy-of select="name"/><br/>(5)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_NOOPT']">
           <td><xsl:copy-of select="name"/><br/>(6)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_PROT']">
           <td><xsl:copy-of select="name"/><br/>(7)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_OVRFLOW']">
           <td><xsl:copy-of select="name"/><br/>(8)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_PARAM']">
           <td><xsl:copy-of select="name"/><br/>(9)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_BUFFER']">
           <td><xsl:copy-of select="name"/><br/>(10)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_PATH']">
           <td><xsl:copy-of select="name"/><br/>(11)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_MODE']">
           <td><xsl:copy-of select="name"/><br/>(12)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_REJECT']">
           <td><xsl:copy-of select="name"/><br/>(13)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_DTSRVR']">
           <td><xsl:copy-of select="name"/><br/>(14)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_ALARM']">
           <td><xsl:copy-of select="name"/><br/>(15)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_STOP']">
           <td><xsl:copy-of select="name"/><br/>(16)</td>
        </xsl:when>
        <xsl:when test="name[.='EW_PASSWD']">
           <td><xsl:copy-of select="name"/><br/>(17)</td>
        </xsl:when>
        <xsl:otherwise>
           <td><xsl:copy-of select="name"/></td>
        </xsl:otherwise>
    </xsl:choose>

    <td><xsl:copy-of select="content"/></td>
  </tr>
</xsl:template>

<xsl:template match="root/func/errcode/item[@class=&quot;ois&quot;] | root/func/errcode/is/item[@class=&quot;ois&quot;]">
</xsl:template>




<!-- CNCオプションのテンプレート -->
<xsl:template match="root/func/option">
  <p class="label">CNC option</p>
  <p><xsl:copy-of select="cmn"/></p>
  <xsl:if test="count(need) != 0">
    <p>This function need the following CNC option.</p>
    <!-- 本機能には以下のオプションが必要です。-->
    <ul>
      <p><xsl:copy-of select="need"/></p>
    </ul>
  </xsl:if>
  <xsl:if test="count(type) != 0">
    <p>And this function is related to the following CNC option.</p>
    <!-- また以下のオプションが関係します。-->
    <ul>
      <p><xsl:copy-of select="type"/></p>
    </ul>
  </xsl:if>
  <xsl:if test="count(hssb) != 0">
    <p>For HSSB connection,</p>
    <!-- HSSB接続では、-->
    <ul>
      <p><xsl:copy-of select="hssb"/></p>
    </ul>
  </xsl:if>
  <xsl:if test="count(ethr) != 0">
    <p>For Ethernet connection,</p>
    <!-- Ethernet接続では、-->
    <ul>
      <p><xsl:copy-of select="ethr"/></p>
    </ul>
  </xsl:if>
  <p><xsl:copy-of select="add"/></p>
</xsl:template>





<!-- CNCパラメータのテンプレート -->
<xsl:template match="root/func/parameter">
  <p class="label">CNC parameter</p>
  <xsl:if test="count(*) = 0">
    <p>This function is not related to CNC parameter.</p>
    <!-- 本機能にパラメータは関係しません。-->
  </xsl:if>
  <xsl:if test="count(*) != 0">
    <xsl:if test="(count(dtsv) = 0) and (count(type) = 0)">
      <p>This function is not related to CNC parameter.</p>
      <!-- 本機能にパラメータは関係しません。-->
    </xsl:if>
    <xsl:if test="count(dtsv) != 0">
      <p>This function is related to the following CNC parameter.<br/>
      See the manual of DATA SERVER for details.</p>
      <!-- 本機能には以下のパラメータが関係します。
      詳細はDATA SERVER取扱説明書を参照して下さい。-->
      <p><xsl:copy-of select="dtsv"/></p>
    </xsl:if>
    <xsl:if test="count(fast) != 0">
      <p>This function is related to the following CNC parameter.<br/>
      See the manual of FAST Ethernet for details.</p>
      <!-- 本機能には以下のパラメータが関係します。
      詳細はDATA SERVER取扱説明書を参照して下さい。-->
      <p><xsl:copy-of select="fast"/></p>
    </xsl:if>
    <xsl:if test="count(type) != 0">
      <p>This function is related to the following CNC parameter.<br/>
      See the manual of CNC parameter for details.</p>
      <!-- 本機能には以下のパラメータが関係します。
      詳細は CNCのパラメータ説明書を参照して下さい。-->
      <p><xsl:copy-of select="type"/></p>
    </xsl:if>
    <p><xsl:copy-of select="add"/></p>
  </xsl:if>
</xsl:template>





<!-- CNCモードのテンプレート -->
<xsl:template match="root/func/mode">
  <p class="label">CNC mode</p>
  <xsl:if test="count(*) = 0">
    <p>This function can be used in any CNC mode.</p>
    <!-- すべてのモードで使用できます。-->
  </xsl:if>
  <xsl:if test="count(*) != 0">
    <p><xsl:copy-of select="cmn"/></p>
    <p><xsl:copy-of select="add"/></p>
  </xsl:if>
</xsl:template>





<!-- 対応機種のテンプレート -->
<xsl:template match="root/func/support">
  <p class="label">Available CNC</p>
  <table border="1">
    <colgroup width="100em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
  <tr>
    <th></th>
    <th>0i-A</th>
    <th>0i-B/C(Note)</th>
    <th>0i-D</th>
    <th>0i-F</th>
    <th>15</th>
    <th>15i</th>
    <th>16</th>
    <th>18</th>
    <th>21</th>
    <th>16i-A</th>
    <th>18i-A</th>
    <th>21i-A</th>
    <th>16i-B</th>
    <th>18i-B</th>
    <th>21i-B</th>
    <th>30i-A</th>
    <th>30i-B</th>
  </tr>
  <tr><td>M (Machining)</td><xsl:apply-templates select="m"/></tr>
  <tr><td>T (Turning)  </td><xsl:apply-templates select="t"/></tr>
  <tr><td>LC (Loader)  </td><xsl:apply-templates select="lc"/></tr>
  </table><p/>

  <table border="1">
    <colgroup width="100em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
    <colgroup align="center" width="40em"/>
  <tr>
    <th></th><th>0i-D</th><th>0i-F</th><th>16i</th><th>18i</th><th>30i-A</th><th>30i-B</th>
  </tr>
  <tr><td>P (Punch press)</td><xsl:apply-templates select="p"/></tr>
  <tr><td>L (Laser)      </td><xsl:apply-templates select="l"/></tr>
  <tr><td>W (Wire)       </td><xsl:apply-templates select="w"/></tr>
  </table><p/>

  <br/>
  <table border="1">
    <colgroup width="100em"/>
    <colgroup align="center" width="86em"/>
    <colgroup align="center" width="86em"/>
<!--  <tr>
    <th></th><th>Power Mate i</th>
  </tr> -->
  <tr><td>Power Mate i-D</td><xsl:apply-templates select="pw/d"/></tr>
  <tr><td>Power Mate i-H</td><xsl:apply-templates select="pw/h"/></tr>
  <tr><td>Power Motion i-A</td><xsl:apply-templates select="pw/a"/></tr>
  </table><p/>
  
  <xsl:copy-of select="original"/>

  <table class="dd">
    <tr> <td></td> <td>"O"</td> <td>:</td> <td>Both Ethernet and HSSB</td> </tr>
    <tr> <td></td> <td>"E"</td> <td>:</td> <td>Ethernet</td> </tr>
    <tr> <td></td> <td>"H"</td> <td>:</td> <td>HSSB</td> </tr>
    <tr> <td></td> <td>"X"</td> <td>:</td> <td>Cannot be used</td> </tr>
    <tr> <td></td> <td>"-"</td> <td>:</td> <td>None</td> </tr>
  </table>
  <p style="background-color:mistyrose;">Note) 0i-C does not support the HSSB function.</p>
  <p><xsl:copy-of select="add"/></p>
</xsl:template>

<xsl:template match="m">
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs0ia"/>  </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs0ib"/>  </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs0id"/>  </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs0if"/>  </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs15"/>   </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs15i"/>  </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs16"/>   </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs18"/>   </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs21"/>   </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs16ia"/> </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs18ia"/> </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs21ia"/> </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs16ib"/> </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs18ib"/> </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs21ib"/> </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs30ia"/> </xsl:call-template>
  <xsl:call-template name="support"> <xsl:with-param name="item" select="fs30ib"/> </xsl:call-template>
</xsl:template>

<xsl:template match="t">
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs0ia"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs0ib"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs0id"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs0if"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs15"/>   </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs15i"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs21"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs21ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16ib"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18ib"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs21ib"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs30ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs30ib"/> </xsl:call-template>
</xsl:template>

<xsl:template match="lc">
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0ia"/>  </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0ib"/>  </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0id"/>  </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0if"/>  </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs15"/>   </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs15i"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs21"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs21ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16ib"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18ib"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs21ib"/> </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs30ia"/> </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs30ib"/> </xsl:call-template>
</xsl:template>

<xsl:template match="p">
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs0id"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs0if"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16i"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18i"/>   </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs30ia"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs30ib"/>  </xsl:call-template>
</xsl:template>

<xsl:template match="l">
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0id"/>   </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0if"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16i"/>  </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs18i"/>  </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs30ia"/> </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs30ib"/> </xsl:call-template>
</xsl:template>

<xsl:template match="w">
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0id"/>   </xsl:call-template>
  <xsl:call-template name="support2"> <xsl:with-param name="item" select="fs0if"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs16i"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs18i"/>   </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs30ia"/>  </xsl:call-template>
  <xsl:call-template name="support">  <xsl:with-param name="item" select="fs30ib"/>  </xsl:call-template>
</xsl:template>

<xsl:template match="pw/d | pw/h | pw/a">
  <xsl:call-template name="support"> <xsl:with-param name="item" select="."/>      </xsl:call-template>
</xsl:template>


<xsl:template name="support">
  <xsl:param name="item"/>
  <td>
  <xsl:attribute name="class">
    <xsl:choose>
      <xsl:when test="$item[. ='O']">support</xsl:when>
      <xsl:when test="$item[. ='E']">support</xsl:when>
      <xsl:when test="$item[. ='H']">support</xsl:when>
    </xsl:choose>
  </xsl:attribute>
  <xsl:copy-of select="$item"/>
  </td>
</xsl:template>

<xsl:template name="support2">
  <xsl:param name="item"/>
  <td>
  -
  </td>
</xsl:template>



<!-- 参照のテンプレート -->
<xsl:template match="root/func/reference">
  <xsl:if test="count(*) != 0"> 
    <p class="label">See Also</p>
    <p><xsl:apply-templates select="item"/></p>
  </xsl:if>
</xsl:template>


<xsl:template match="root/func/reference/item">
  <a class="func" target="_parent">
    <xsl:attribute name="href"><xsl:value-of select="file"/></xsl:attribute>
    <xsl:value-of select="name"/>
  </a>&#160;&#160;
</xsl:template>





<!-- プログラム例のテンプレート -->
<xsl:template match="root/func/example">
  <xsl:if test="count(*) != 0">
    <p class="label">Example(C Language)</p>
    <p><xsl:copy-of select="common"/></p>
    <pre><xsl:value-of select="vc"/></pre>
    <p class="label">Example(C#)</p>
    <pre><xsl:value-of select="vcs"/></pre>
  </xsl:if>
</xsl:template>



</xsl:stylesheet>
