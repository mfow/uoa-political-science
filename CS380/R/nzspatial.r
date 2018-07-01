electorateCount = 70;
electorateIds = 1:electorateCount;
parties = c(
	"Labour",
	"New Zealand First Party",
	"Conservative Party",
	"National Party",
	"Green Party",
	"ACT New Zealand",
	"Alliance",
	"Aotearoa Legalise Cannabis Party",
	"Democrats for Social Credit",
	"Libertarianz",
	"Mana",
	"MÄori Party",
	"United Future"
);

nzspatial.df = data.frame()
districtNames = vector()
for (id in electorateIds)
{
	htmlSource = paste("http://www.electionresults.govt.nz/electionresults_2011/electorate-",id,".html",sep="");
	html = readLines(htmlSource)
	
	titleIndex = grep("Official Count Results -- ", html)
	districtName = strsplit(strsplit(html[titleIndex]," -- ")[[1]][2],"</title>")[[1]][1];
	districtNames[id] = districtName;
	
	for (pId in 1:length(parties))
	{
		p = parties[pId];
		index = grep(p, html)
		voteCount = as.numeric(sub(",","",sub("^ +", "", strsplit(strsplit(html[index],"<td align=\"right\">")[[1]][2],"</td")[[1]][1])))	
		nzspatial.df[id,pId] = voteCount;
	}
}

names(nzspatial.df) = parties;
nzspatial.df$name = districtNames;
