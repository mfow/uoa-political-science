nz.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/CS380/bin/Debug/out/nzsim_3.csv", sep = ",", skip = 0, header=T)
nz.df$X <- NULL
nz.df[1:5,]

t1 = 0.00;
t2 = 0.05;

loosemorehanby = nz.df$loosemorehanby;
gallagher = nz.df$gallagher;
shapleySumSquares = nz.df$governability;
seatPropSumSquares = 1.0 / nz.df$enp;
enp = nz.df$enp;
shapleyEntropy = -1.0 / (nz.df$entropy + 0.1);
seatPropEntropy = -1.0 / (nz.df$entropy2 + 0.1);
randOffset = rnorm(nrow(nz.df)) / 10000.0;

cols = c("red", "blue");

for (t in 0:7)
{
	t_1 = t;
	t_2 = t + 1;
	
	t1 = t_1 / 100.0;
	t2 = t_2 / 100.0;
	
	condition = ((abs(nz.df$threshold - t1) < 0.001) | (abs(nz.df$threshold - t2) < 0.001));
	
	#windows()
	#plot.new()
	#png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_sumsquares.png",sep=""))
	#plot(loosemorehanby[condition] + randOffset[condition], shapleySumSquares[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (shapley shubik sum of squares)")
	#dev.off()
	
	#windows()
	#plot.new()
	#png(file=paste("submission/", t_1,"-",t_2, "_gallagher_sumsquares.png",sep=""))
	#plot(gallagher[condition] + randOffset[condition], shapleySumSquares[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (shapley shubik sum of squares)")
	#dev.off()
	
	#windows()
	#plot.new()
	#png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_entropy.png",sep=""))
	#plot(loosemorehanby[condition] + randOffset[condition], shapleyEntropy[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (shapley shubik entropy)")
	#dev.off()
	
	#windows()
	#plot.new()
	#png(file=paste("submission/", t_1,"-",t_2, "_gallagher_entropy.png",sep=""))
	#plot(gallagher[condition] + randOffset[condition], shapleyEntropy[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (shapley shubik entropy)")
	#dev.off()

## 	windows()
## 	plot.new()
## 	png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_entropy.png",sep=""))
## 	plot(loosemorehanby[condition] + randOffset[condition], seatPropEntropy[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (seat fractions entropy)")
## 	dev.off()
	
## 	windows()
## 	plot.new()
## 	png(file=paste("submission/", t_1,"-",t_2, "_gallagher_entropy.png",sep=""))
## 	plot(gallagher[condition] + randOffset[condition], seatPropEntropy[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (seat fractions entropy)")
##	dev.off()


	windows()
	plot.new()
	png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_enp.png",sep=""))
	plot(loosemorehanby[condition] + randOffset[condition], enp[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (enp)")
	dev.off()
	
	windows()
	plot.new()
	png(file=paste("submission/", t_1,"-",t_2, "_gallagher_enp.png",sep=""))
	plot(gallagher[condition] + randOffset[condition], enp[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (enp)")
	dev.off()
	
	# windows()
	# plot.new()
	# png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_entropy_crop.png",sep=""))
	# plot(loosemorehanby[condition] + randOffset[condition], shapleyEntropy[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (entropy)", ylim=c(-0.5, 1))
	# dev.off()
	
	# windows()
	# plot.new()
	# png(file=paste("submission/", t_1,"-",t_2, "_gallagher_entropy_crop.png",sep=""))
	# plot(gallagher[condition] + randOffset[condition], shapleyEntropy[condition], col=cols[(nz.df$threshold[condition] == t2) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (entropy)", ylim=c(-0.5,1))
	# dev.off()
}

# plot of 0 to 5%
condition = (nz.df$threshold >= 0.02) & (nz.df$threshold <= 0.05);
cols2 = c("black", "red", "orange", "green", "blue", "purple");

t1 = 0.02;
t2 = 0.05;
t_1 = 2;
t_2 = 5;

#	windows()
#	plot.new()
#	png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_sumsquares.png",sep=""))
#	plot(loosemorehanby[condition] + randOffset[condition], shapleySumSquares[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (shapley shubik sum of squares)")
#	dev.off()
	
#	windows()
#	plot.new()
#	png(file=paste("submission/", t_1,"-",t_2, "_gallagher_sumsquares.png",sep=""))
#	plot(gallagher[condition] + randOffset[condition], shapleySumSquares[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (shapley shubik sum of squares)")
#	dev.off()
	
#	windows()
#	plot.new()
#	png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_entropy.png",sep=""))
#	plot(loosemorehanby[condition] + randOffset[condition], shapleyEntropy[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (shapley shubik entropy)")
#	dev.off()
	
#	windows()
#	plot.new()
#	png(file=paste("submission/", t_1,"-",t_2, "_gallagher_entropy.png",sep=""))
#	plot(gallagher[condition] + randOffset[condition], shapleyEntropy[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (shapley shubik entropy)")
#	dev.off()
	

## 	windows()
## 	plot.new()
## 	png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_entropy2.png",sep=""))
## 	plot(loosemorehanby[condition] + randOffset[condition], seatPropEntropy[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (seat fractions entropy)")
## 	dev.off()
	
## 	windows()
## 	plot.new()
## 	png(file=paste("submission/", t_1,"-",t_2, "_gallagher_entropy2.png",sep=""))
## 	plot(gallagher[condition] + randOffset[condition], seatPropEntropy[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (seat fractions entropy)")
## 	dev.off()
	
	windows()
	plot.new()
	png(file=paste("submission/", t_1,"-",t_2, "_loosemorehanby_enp.png",sep=""))
	plot(loosemorehanby[condition] + randOffset[condition], enp[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (loosemorehanby)", ylab="governability (enp)")
	dev.off()
	
	windows()
	plot.new()
	png(file=paste("submission/", t_1,"-",t_2, "_gallagher_enp.png",sep=""))
	plot(gallagher[condition] + randOffset[condition], enp[condition], col=cols2[(nz.df$threshold[condition] * 100.0) + 1], main=paste(t1, "-", t2), xlab="proportionality (gallagher)", ylab="governability (enp)")
	dev.off()	
	
	
	