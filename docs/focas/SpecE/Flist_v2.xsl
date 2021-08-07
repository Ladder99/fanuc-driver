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
      <a  target="_parent">
        <xsl:attribute name="href">
          <xsl:value-of select="concat(fpage, 'l')" />
        </xsl:attribute>
        <xsl:value-of select="fname"/>
      </a>
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
  </head>
  <body bgcolor="#FFFFFF">
    <div class="text">
      <xsl:apply-templates select="root/chapter" mode="putcontent"/>
    </div>
  </body>
</html>

</xsl:template>

</xsl:stylesheet>
