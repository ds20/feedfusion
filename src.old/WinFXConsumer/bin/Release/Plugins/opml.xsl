<?xml version="1.0"?>
  <xsl:stylesheet 
      xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="html" version="1.0" 
      indent="yes" encoding="iso-8859-1"/>
  <xsl:template match="/">
    <html><body>
      <div style=
         "padding: 1em;background-color: #fafafa; border: 1px solid #cfcfcf;">

	<xsl:for-each select="opml/head">
	  <div>
	    <p><h2 style="color:blue"><xsl:value-of select="title"/></h2>
            </p>
	    <h4 style="color:red"><xsl:text> Created:  </xsl:text><xsl:value-of select="dateCreated"/></h4>
	    <h4 style="color:red"><xsl:text> Modified:  </xsl:text><xsl:value-of select="dateModified"/></h4>
	    <h4 style="color:red"><xsl:text> author </xsl:text><xsl:value-of select="ownerName"/></h4>
	    <h4 style="color:red"><xsl:value-of select="ownerEmail"/></h4>
	  </div>
	</xsl:for-each>

        <xsl:for-each select="opml/body">
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
		<p>
		<xsl:apply-templates select="outline"/>
		</p>            
          </div>
        </xsl:for-each>
      </div>
    </body></html>
  </xsl:template>

<xsl:template match="outline"> 
     <div style="color:purple">           
          <xsl:text> text: </xsl:text> 
          <xsl:value-of select="@text"/> 
	<p>
	  <xsl:text> title: </xsl:text> 
          <xsl:value-of select="@title"/> 
	  <xsl:text> -----> about: </xsl:text> 
 	  <xsl:value-of disable-output-escaping="yes" 
                                               select="@description"/>
          
	</p>
	  <p style="margin:0; padding:0em 0em 1em 0em"><a target="_blank">
              <xsl:attribute name="href">
                <xsl:value-of select="@htmlUrl"/>
              </xsl:attribute>
              <xsl:value-of select="@htmlUrl"/>
            </a></p>
	  <p style="margin:0; padding:0em 0em 1em 0em"><a target="_blank">
              <xsl:attribute name="href">
                <xsl:value-of select="@xmlUrl"/>
              </xsl:attribute>
              <xsl:value-of select="@xmlUrl"/>
            </a></p>

     </div> 
</xsl:template>




</xsl:stylesheet>