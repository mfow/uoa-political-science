spatial.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/CS380/bin/Debug/out/pcavalidation2.csv", sep = ",", skip = 0, header=T)
spatial.df$X <- NULL
spatial.df[1:5,]

windows()
plot.new()
png(file=paste("spatialvar1_pcavar1.png",sep=""))
plot(spatial.df$spatialvar1, spatial.df$pcavar1, xlab="Spatial Dimension 1 Standard Deviation", ylab="Proportion of variance explained by 1st Principal Component")
dev.off()

windows()
plot.new()
plot(spatial.df$spatialvar2, spatial.df$pcavar2)

windows()
plot.new()
plot(spatial.df$spatialvar3, spatial.df$pcavar3)

model = lm(pcavar1~spatialvar1, data=spatial.df)
summary(model)
anova(model)

model = lm(pcavar1~I(spatialvar1)+I(spatialvar1^0.5), data=spatial.df)
summary(model)
anova(model)

spatial.df$spatialSum = spatial.df$spatialvar1 + spatial.df$spatialvar2 + spatial.df$spatialvar3 + spatial.df$spatialvar4 + spatial.df$spatialvar5 + spatial.df$spatialvar6 + spatial.df$spatialvar7 + spatial.df$spatialvar8;
spatial.df$effectiveSpatial =
	1.0 / (
	((spatial.df$spatialvar1/spatial.df$spatialSum)^2)+
	((spatial.df$spatialvar2/spatial.df$spatialSum)^2)+
	((spatial.df$spatialvar3/spatial.df$spatialSum)^2)+
	((spatial.df$spatialvar4/spatial.df$spatialSum)^2)+
	((spatial.df$spatialvar5/spatial.df$spatialSum)^2)+
	((spatial.df$spatialvar6/spatial.df$spatialSum)^2)+
	((spatial.df$spatialvar7/spatial.df$spatialSum)^2)+
	((spatial.df$spatialvar8/spatial.df$spatialSum)^2));

spatial.df$effectivePCA = 
	1.0 / (
	(spatial.df$pcavar1 ^ 2) + 
	(spatial.df$pcavar2 ^ 2) + 
	(spatial.df$pcavar3 ^ 2) + 
	(spatial.df$pcavar4 ^ 2) + 
	(spatial.df$pcavar5 ^ 2) + 
	(spatial.df$pcavar6 ^ 2) + 
	(spatial.df$pcavar7 ^ 2) + 
	(spatial.df$pcavar8 ^ 2));

plot(spatial.df$effectiveSpatial, spatial.df$effectivePCA)
cor(spatial.df$effectiveSpatial, spatial.df$effectivePCA)

windows()
plot.new()
plot(spatial.df$spatialvar1, resid(model))

windows()
plot.new()
plot(spatial.df$pcavar1, resid(model))

normFactor = (
spatial.df$spatialvar1+
spatial.df$spatialvar2+
spatial.df$spatialvar3+
spatial.df$spatialvar4+
spatial.df$spatialvar5+
spatial.df$spatialvar6+
spatial.df$spatialvar7+
spatial.df$spatialvar8)


plot(spatial.df$spatialvar1 / normFactor, spatial.df$pcavar1, xlab="Spatial Dimension 1 Standard Deviation", ylab="Proportion of variance explained by 1st Principal Component")


model = lm(pcavar1~I(spatialvar1/normFactor), data=spatial.df)
summary(model)
anova(model)

model = lm(pcavar1~I(spatialvar1/normFactor)+I((spatialvar1/normFactor)^0.5), data=spatial.df)
summary(model)
anova(model)
