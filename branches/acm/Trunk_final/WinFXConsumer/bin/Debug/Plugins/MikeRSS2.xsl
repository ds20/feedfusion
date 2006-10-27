<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
 
 <xsl:template match="/">
	<xsl:apply-templates /> 
 </xsl:template>
 
<xsl:template match="/rss/channel">
	<html>
	<body>
	
	<h3>
	
	<xsl:if test="image">
		<a target="_blank">
			<xsl:if test="image/link"><xsl:attribute name="href"><xsl:value-of select="image/link" /></xsl:attribute></xsl:if>
			<img border="0">
			<xsl:attribute name="src"><xsl:value-of select="image/url" /></xsl:attribute>
			<xsl:attribute name="alt"><xsl:value-of select="image/title" /></xsl:attribute>
			<xsl:if test="image/width">
				<xsl:attribute name="width"><xsl:value-of select="image/width" /></xsl:attribute>
			</xsl:if>
			<xsl:if test="image/height">
				<xsl:attribute name="height"><xsl:value-of select="image/height" /></xsl:attribute>
			</xsl:if>
			</img>
		</a>
		<br />
	</xsl:if>
	
	<a target="_blank">
	<xsl:attribute name="href">
		<xsl:value-of select="link"/>
    </xsl:attribute>
    <xsl:value-of select="title"/>
    </a>
	<br />
	<xsl:value-of select="description" disable-output-escaping="yes" />
	</h3>
	
	<xsl:if test="pubDate">
		<i>Feed updated: <xsl:value-of select="pubDate" /> </i> <br />
	</xsl:if>
	<xsl:if test="lastBuildDate">
		<i>Channel updated: <xsl:value-of select="lastBuildDate" /> </i> <br />
	</xsl:if>
	<xsl:if test="language">
		Language: <xsl:value-of select="language" /> <br />
	</xsl:if>
	
	<br />
	
	<xsl:for-each select="item">
		  <a target="_blank">
		  <xsl:attribute name="href">
		  	<xsl:value-of select="link" />
		  </xsl:attribute>
		  <xsl:value-of select="title" disable-output-escaping="yes" />
		  </a>
		  <br />
		  <xsl:value-of select="description" disable-output-escaping="yes" />
		  <xsl:if test="pubDate"><br />Published: <xsl:value-of select="pubDate" disable-output-escaping="yes" /></xsl:if>
		  <hr />
    </xsl:for-each>
	
	</body>
	</html>
</xsl:template>


</xsl:stylesheet>