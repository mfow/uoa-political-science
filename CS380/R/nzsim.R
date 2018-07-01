nz.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/CS380/bin/Debug/out/nzsim2pure_pred5.csv", sep = ",", skip = 0, header=T)
nz.df$X <- NULL
nz.df[1:5,]

plot(nz.df$threshold, nz.df$loosemorehanby)
jpeg(file="thresholdloosemorehanby.jpg")
plot(nz.df$threshold, nz.df$governability)
jpeg(file="thresholdgovernability.jpg")

plot(nz.df$loosemorehanby, nz.df$governability)
jpeg(file="loosemorehanbygovernability.jpg")

plot(1.0 / nz.df$enp, nz.df$governability)

# https://stat.ethz.ch/pipermail/r-help/2009-March/192050.html
col<- rainbow(255,end=5/6)

colid <- function( x, range=NULL, depth=255 )
{
    if ( is.null( range ) )
        y <- as.integer(x-min(x))/(max(x)-min(x))*depth+1
    else
    {
        y <- as.integer(x-range[1])/(range[2]-range[1])*depth+1
        y[ which( y < range[1] ) ] <- 1
        y[ which( y > range[2] ) ] <- depth
    }
    y
}

windows();
par(mfrow=c(3, 3))

years = c(2002, 2005, 2008, 2011)

for (year in years)
{
	plot.new()
	condition = (nz.df$year == year)
	
	randOffset = rnorm(nrow(nz.df)) / 10000.0
	plot(nz.df$loosemorehanby[condition] + randOffset[condition], 1.0 - nz.df$governability[condition], col=col[colid(nz.df$threshold[condition] * 1000)], main=year, xlab="proportionality", ylab="governability")
	
	jpeg(file=paste(year, " prop governability.jpg"))
}

plot((0:254 / 255) * max(nz.df$threshold),1:255 * 0, ylim=c(0.0,0.0), col=col, xlab="threshold")





windows();
par(mfrow=c(3, 3))

years = c(2002, 2005, 2008, 2011)
randOffset = rnorm(nrow(nz.df)) / 10000.0

for (year in years)
{
	plot.new()
	condition = (nz.df$year == year) & (nz.df$threshold < 0.06)
	plot(nz.df$loosemorehanby[condition] + randOffset[condition], 1.0 - nz.df$governability[condition], col=col[colid(nz.df$threshold[condition] * 1000)], main=year, xlab="proportionality", ylab="governability")
	
	jpeg(file=paste(year, " prop governability threshold under 0.06.jpg"))
}

plot((0:254 / 255) * 0.06,1:255 * 0, col=col, xlab="threshold")

condition = (nz.df$threshold < 1.0)

windows();
par(mfrow=c(1, 1))
plot(nz.df$loosemorehanby[condition] + randOffset[condition], 1.0 - nz.df$governability[condition], col=col[colid(nz.df$threshold[condition] * 1000)], xlab="proportionality", ylab="governability", main="2002-2011")
jpeg(file="20022011 prop governability.jpg")

condition = (nz.df$threshold < 0.06)

windows();
par(mfrow=c(1, 1))
plot(nz.df$loosemorehanby[condition] + randOffset[condition], 1.0 - nz.df$governability[condition], col=col[colid(nz.df$threshold[condition] * 1000)], xlab="proportionality", ylab="governability", main="2002-2011, threshold<0.06")
jpeg(file="20022011 threshold_0.06.jpg")

windows();
par(mfrow=c(1, 1))
plot(-nz.df$entropy[condition] + randOffset[condition], 1.0 - nz.df$governability[condition], col=col[colid(nz.df$threshold[condition] * 1000)], xlab="proportionality (entropy)", ylab="governability", main="2002-2011, entropy. threshold<0.06")
jpeg(file="20022011 entropy threshold_0.06.jpg")

windows();
par(mfrow=c(1, 1))
plot(nz.df$maxdelta[condition] + randOffset[condition], 1.0 - nz.df$governability[condition], col=col[colid(nz.df$threshold[condition] * 1000)], xlab="proportionality (entropy)", ylab="governability", main="2002-2011, maxdelta. threshold<0.06")
jpeg(file="20022011 maxdelta threshold_0.06.jpg")

windows();
plot(-nz.df$entropy, nz.df$loosemorehanby)
jpeg(file="entropy_loosemorehanby.jpg")

windows();
plot(nz.df$maxdelta, nz.df$loosemorehanby)
jpeg(file="maxdelta_loosemorehanby.jpg")