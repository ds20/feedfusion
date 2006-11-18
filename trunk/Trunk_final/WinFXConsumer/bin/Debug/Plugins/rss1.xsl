<?xml version="1.0"?>
  <xsl:stylesheet 
      xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="html" 
      version="1.0" indent="yes" encoding="iso-8859-1"/>
  <xsl:template match="/">
    <html><body>
      <div style=
         "padding: 1em;background-color: #fafafa; border: 1px solid #cfcfcf;">
        
	<xsl:for-each select="*/*[local-name()='channel']">
	  <div>
        <h2 style="color:blue; margin-left:50">
        <a>
            <xsl:attribute name="href">
                <xsl:value-of select="./*[local-name()='link']"/>
            </xsl:attribute>
            <xsl:value-of select="./*[local-name()='title']"/>
        </a>
        </h2>
	    <h4 style="color:blue; margin-left:50"><xsl:value-of select="./*[local-name()='description']"/></h4>
	  </div>
	</xsl:for-each>


	<xsl:for-each select="*/*[local-name()='item']">
          <xsl:variable name="stl">
            <xsl:text/>background-color: #efeff5; 
               border: 1px solid #cfcfcf;padding: 0em 1em 0em; margin:
            <xsl:text/>
            <xsl:choose>
              <xsl:when test="position()=last()"> 0em</xsl:when>
              <xsl:otherwise> 0em 0em 1em 0em</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <div>
            <xsl:attribute name="style"><xsl:value-of select="$stl"/>
            </xsl:attribute>
            <p><h3 style="color:maroon">
                <xsl:value-of select="./*[local-name()='title']"/>
            </h3></p>
            <p><xsl:value-of disable-output-escaping="yes" 
                select="./*[local-name()='description']"/>
            <br/><br/>
            <xsl:variable name="pub" select="*[local-name()='date']"/>
            <xsl:variable name="pub_date" 
                select="concat(substring($pub, 0, 11), ', ', 
                               substring($pub, 12, 8), ' (GMT+',  
                               substring($pub, 21, 5), ')')"/>
            <xsl:if test="count($pub) > 0">
              <div align="right" style="margin:0em; padding:0em 0em 0em 0em;">
              <xsl:value-of select="$pub_date"/></div>
            </xsl:if>

 	  <i>         
	    <xsl:text> | </xsl:text>
     	  </i> 
	    <xsl:apply-templates select="category"/>

            <a target="_blank">
              <xsl:attribute name="href">
                <xsl:value-of select="./*[local-name()='link']"/>
              </xsl:attribute><xsl:value-of select="./*[local-name()='link']"/>
            </a>
	    <a target="_blank">
              <xsl:attribute name="href">
                <xsl:value-of select="./*[local-name()='comments']"/>
              </xsl:attribute><xsl:value-of select="./*[local-name()='comments']"/>
            </a></p>
          </div>
        </xsl:for-each>
      </div>
    </body></html>
  </xsl:template>

<xsl:template match="category"> 
     <i> 
          <xsl:value-of select="."/> 
	  <xsl:text> | </xsl:text>

     </i> 
</xsl:template>
</xsl:stylesheet>