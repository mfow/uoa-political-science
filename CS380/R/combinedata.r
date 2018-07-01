datasource = "C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial_3/spatial3_";
datasource_end = "_stv.csv";
datawrite = "C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial_3/spatial3.csv";
seatCount = 120;

data.df = NULL;

for (i in 1:seatCount)
{
	j = (floor(seatCount/i) * i);
	
	if (j == seatCount)
	{
		results.df = read.table(paste(datasource,i,datasource_end,sep=""), sep = ",", skip = 0, header=T)
		results.df$dm = i
		data.df = merge(results.df, data.df, all=TRUE)
	}
}

nrow(data.df)

write.table(data.df,file=datawrite,sep=",",row.names=TRUE,col.names=NA)
