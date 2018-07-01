library(ggplot2)

tinyColor = "firebrick1";
smallColor = "green3";
mediumColor = "dodgerblue3";
largeColor = "mediumorchid2";

map = function(v, f)
{
	result = NULL
	
	for (i in 1:length(v))
	{
		result[i] = f(v[i]);
	}
	
	return (result);
}

datafiles.df = data.frame(
	datasource = c(
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results2/pd_spatial2_results.csv",
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial_1/spatial1.csv",
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial_2/spatial2.csv",
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial_3/spatial3.csv",
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results5/urnstv.csv",
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial_2pure/spatial2pure.csv",
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial_2large/spatial2large.csv",
		"C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/spatial2_sm/spatial2sm.csv"
	),
	
	name=c(
		"pd_spatial2",
		"stv_spatial1",
		"stv_spatial2",
		"stv_spatial3",
		"stv_urn",
		"stv_spatial2pure",
		"spatial2large",
		"sm_spatial2"
	),stringsAsFactors=FALSE
)

for (fairnessId in c(0,1))
{
	for (stabilityId in c(0,1,2))
	{
		bestdmoverall.df = data.frame()
		
		for(datasetId in 1:nrow(datafiles.df))
		{
			results.df = read.table(datafiles.df$datasource[datasetId], sep = ",", skip = 0, header=T)
			datasetName = datafiles.df$name[datasetId]

			results.df$X <- NULL
			results.df[1:5,]

			fairnessName = "UNKNOWN"
			if (fairnessId==0)
			{
				results.df$fairness = results.df$gallagher;
				fairnessName = "gallagher";
			}
			
			if (fairnessId==1)
			{
				results.df$fairness = results.df$loosemorehanby;
				fairnessName = "loosemorehanby";
			}

			stabilityName = "UNKNOWN"
			if (stabilityId==0)
			{
				results.df$stability = results.df$enp-1;
				stabilityName = "enp";
			}
			
			if (stabilityId==1)
			{
				results.df$stability = (1/results.df$governability)-1;
				stabilityName = "enpsspi";
			}

			if (stabilityId==2)
			{
				results.df$stability = -results.df$entropy;
				stabilityName = "entropysspi";
			}
			
			#plot.new()
			pdf(paste("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/Report/images/", datasetName,"_",fairnessName,"_",stabilityName, ".pdf",sep=""))

			split.screen(c(2,2))

			screen(1)
			results.df$col1 = largeColor
			results.df$col1[results.df$dm<30] = mediumColor
			results.df$col1[results.df$dm<10] = smallColor
			results.df$col1[results.df$dm==1] = tinyColor
			rOrder = order(rnorm(nrow(results.df)))
			plot(results.df$fairness[rOrder], results.df$stability[rOrder], xlab="Unfairness", ylab="Instability",col=results.df$col1[rOrder],main="Simulated Elections") 
			results.df$col1 <- NULL

			#Fit fairness
			fairness.fit = lm(fairness~I(dm)+I(dm^2)+I(log(dm)+I(log(dm)^2)), data=results.df)
			summary(fairness.fit)

			#Fit stability
			stability.fit = lm(stability~I(dm)+I(log(dm))+I(dm^2), data=results.df)
			summary(stability.fit)
			
			dummy.df = data.frame(dm=(10:1200)/10)
			dummy.df$fairness = (predict(fairness.fit, dummy.df))
			dummy.df$stability = predict(stability.fit, dummy.df)
			dummy.df$isPredicted = TRUE;
			dummy.df$type=1

			dummy.df$col1 = largeColor
			dummy.df$col1[dummy.df$dm<30] = mediumColor
			dummy.df$col1[dummy.df$dm<10] = smallColor
			dummy.df$col1[dummy.df$dm==1] = tinyColor

			screen(2)
			randOrder = order(rnorm(nrow(dummy.df)))
			plot(dummy.df$fairness[randOrder], dummy.df$stability[randOrder], xlab="Unfairness", ylab="Instability", main="Predicted Elections by DM", col=dummy.df$col1[randOrder])

			#Calculate extremes
			#Note this plot may be misleading. Points on this plot do not necessarily correspond to actual predicted elections.
			extreme.df = data.frame(dm=unique(results.df$dm))
			for(dm in extreme.df$dm)
			{
				extreme.df$fairness05[extreme.df$dm==dm] = quantile(results.df$fairness[results.df$dm==dm], 0.05)
				extreme.df$fairness95[extreme.df$dm==dm] = quantile(results.df$fairness[results.df$dm==dm], 0.95)
				extreme.df$stability05[extreme.df$dm==dm] = quantile(results.df$stability[results.df$dm==dm], 0.05)
				extreme.df$stability95[extreme.df$dm==dm] = quantile(results.df$stability[results.df$dm==dm], 0.95)
			}

			screen(3)

			extreme.df$col1 = largeColor
			extreme.df$col1[extreme.df$dm<30] = mediumColor
			extreme.df$col1[extreme.df$dm<10] = smallColor
			extreme.df$col1[extreme.df$dm==1] = tinyColor
			plot(extreme.df$fairness95, extreme.df$stability95, xlab="Unfairness", ylab="Instability", main="95th percentile by DM",pch=15,col=extreme.df$col1)

			unknown = rep(NA,length(unique(results.df$dm)))
			actualmeans.df = data.frame(dm=unknown, fairness=unknown, stability=unknown)
			actualmeans.df$dm = unique(results.df$dm)
			actualmeans.df$type=0
			actualmeans.df$isPredicted=FALSE

			for (dm in actualmeans.df$dm)
			{
				actualmeans.df$fairness[actualmeans.df$dm == dm] = mean(results.df$fairness[results.df$dm==dm])
				actualmeans.df$stability[actualmeans.df$dm == dm] = mean(results.df$stability[results.df$dm==dm])
			}

			actualmeans.df$col1 = largeColor
			actualmeans.df$col1[actualmeans.df$dm<30] = mediumColor
			actualmeans.df$col1[actualmeans.df$dm<10] = smallColor
			actualmeans.df$col1[actualmeans.df$dm==1] = tinyColor

			screen(4)
			plot(actualmeans.df$fairness, actualmeans.df$stability, xlab="Unfairness", ylab="Instability", main="Mean Elections by DM",pch=15, col=actualmeans.df$col1)

			dev.off()		
			
			pdf(paste("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/Report/images/", datasetName,"_",fairnessName,"_",stabilityName,"_optimal", ".pdf",sep=""))
			results.df$col2 = "antiquewhite4";
			results.df$col2[results.df$dm==3] = "darkgoldenrod1"
			results.df$col2[results.df$dm==8] = "darkgoldenrod1"
			results.df$col2[results.df$dm==20] = "darkgoldenrod1"
			plot(results.df$fairness[rOrder], results.df$stability[rOrder], xlab="Unfairness", ylab="Instability",col=results.df$col2[rOrder],main="Simulated Elections") 
			dev.off()		
			
			undesirability.df = actualmeans.df;
			undesirability.df$zFairness = (undesirability.df$fairness - min(undesirability.df$fairness))/sd(undesirability.df$fairness)
			undesirability.df$zStability = (undesirability.df$stability - min(undesirability.df$stability))/sd(undesirability.df$stability)
			
			tradeoff.df = data.frame(tCoef=0:100)
			for (tradeoffCoef in tradeoff.df$tCoef)
			{
				tradeoffCoef2 = tradeoffCoef / 100.0;
				
				#Calculate position on line.
				ptX = tradeoffCoef2;
				ptY = 1.0 - tradeoffCoef2;
				distLine = sqrt(ptX * ptX + ptY * ptY);
				xCoef = ptX / distLine;
				yCoef = ptY / distLine;
				
				undesirability.df$undesirability = 
					sqrt(
						(undesirability.df$zFairness * xCoef)^2 +
						(undesirability.df$zStability * yCoef)^2);
				
				tradeoff.df$bestDM[tradeoff.df$tCoef==tradeoffCoef] = undesirability.df$dm[order(undesirability.df$undesirability)][1];
				
				print(undesirability.df)
				print(undesirability.df$dm[order(undesirability.df$undesirability)])
			}	

			print(tradeoff.df)
			
			bestdm.df = data.frame(dm=unique(results.df$dm))
			bestdm.df$count = map(bestdm.df$dm, function(dm) { sum(tradeoff.df$bestDM == dm) })
			bestdm.df$prdensity = bestdm.df$count / sum(bestdm.df$count);
			
			bestdm.df$type = "Large"
			bestdm.df$type[bestdm.df$dm<30] = "Medium"
			bestdm.df$type[bestdm.df$dm<10] = "Small"
			bestdm.df$type[bestdm.df$dm==1] = "Tiny"

			bestdm.df$col1 = largeColor
			bestdm.df$col1[bestdm.df$dm<30] = mediumColor
			bestdm.df$col1[bestdm.df$dm<10] = smallColor
			bestdm.df$col1[bestdm.df$dm==1] = tinyColor
			
			bestdmoverall.df = merge(bestdm.df,bestdmoverall.df,all=TRUE)
			
			plot.new()
			
			pdf(paste("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/Report/images/", datasetName,"_",fairnessName,"_",stabilityName,"_tradeoff",".pdf",sep=""))
			print(ggplot(bestdm.df, aes(dm, prdensity,fill=type)) + geom_bar(stat="identity")+scale_x_continuous(name="District Magnitude")+scale_y_continuous(name="Probability Density")+scale_fill_manual(values = c("Large" = largeColor, "Medium" = mediumColor, "Small" = smallColor, "Tiny" = tinyColor)) )
			dev.off()
			
			tradeoff2.df = tradeoff.df[(tradeoff.df$bestDM > 1) & (tradeoff.df$bestDM < 120),]	
		}
		

		bestdmoverall2.df = data.frame(dm=unique(results.df$dm))
		bestdmoverall2.df$count = map(bestdmoverall2.df$dm, function(dm) { sum(bestdmoverall.df$prdensity[bestdmoverall.df$dm == dm]) })
		bestdmoverall2.df$prdensity = bestdmoverall2.df$count / sum(bestdmoverall2.df$count);
		bestdmoverall2.df$type = "Large"
		bestdmoverall2.df$type[bestdmoverall2.df$dm<30] = "Medium"
		bestdmoverall2.df$type[bestdmoverall2.df$dm<10] = "Small"
		bestdmoverall2.df$type[bestdmoverall2.df$dm==1] = "Tiny"

		bestdmoverall2.df$col1 = largeColor
		bestdmoverall2.df$col1[bestdmoverall2.df$dm<30] = mediumColor
		bestdmoverall2.df$col1[bestdmoverall2.df$dm<10] = smallColor
		bestdmoverall2.df$col1[bestdmoverall2.df$dm==1] = tinyColor
		
		pdf(paste("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/Report/images/", fairnessName,"_",stabilityName,"_tradeoff_overall",".pdf",sep=""))
		print(ggplot(bestdmoverall2.df, aes(dm, prdensity,fill=type)) + geom_bar(stat="identity")+scale_x_continuous(name="District Magnitude")+scale_y_continuous(name="Probability Density")+scale_fill_manual(values = c("Large" = largeColor, "Medium" = mediumColor, "Small" = smallColor, "Tiny" = tinyColor))+geom_text(aes(dm, prdensity,fill=type,label=dm,label=dm)))
		dev.off()
		
		bestdmoverall3.df = bestdmoverall2.df[bestdmoverall2.df$dm > 1 & bestdmoverall2.df$dm < 60,]
		bestdmoverall3.df$prdensity = bestdmoverall3.df$count / sum(bestdmoverall3.df$count);
		pdf(paste("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/Report/images/", fairnessName,"_",stabilityName,"_tradeoff_overall_small",".pdf",sep=""))
		print(ggplot(bestdmoverall3.df, aes(dm, prdensity,fill=type,label=dm)) + geom_bar(stat="identity")+scale_x_continuous(name="District Magnitude")+scale_y_continuous(name="Probability Density")+scale_fill_manual(values = c("Large" = largeColor, "Medium" = mediumColor, "Small" = smallColor, "Tiny" = tinyColor))+geom_text(aes(dm, prdensity,fill=type,label=dm,label=dm)))
		dev.off()

	}
}
