stv.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results/spatial2.csv", sep = ",", skip = 0, header=T)
stv.df$X <- NULL
stv.df[1:5,]



windows()
plot.new()
plot(stv.df$dm, stv.df$gallagher)

windows()
plot.new()
plot(stv.df$dm, stv.df$enp)

stv.df$fairness = stv.df$gallagher;
stv.df$stability = stv.df$enp;

fairness.fit = lm(log(fairness)~I(dm)+I(dm^2), data=stv.df)
stability.fit = lm(stability~I(dm), data=stv.df)

summary(fairness.fit)
summary(stability.fit)


windows()
plot.new()
plot(predict(fairness.fit,data=stv.df), resid(fairness.fit,data=stv.df))

windows()
plot.new()
plot(predict(stability.fit,data=stv.df), resid(stability.fit,data=stv.df))



dummy.df = data.frame(dm=(10:100)/10)
dummy.df$fairness = exp(predict(fairness.fit, dummy.df))
dummy.df$stability = predict(stability.fit, dummy.df)
dummy.df$isPredicted = TRUE;

combined.df = dummy.df;
combined.df = rbind(dummy.df, data.frame(fairness=stv.df$fairness, stability=stv.df$stability, dm=stv.df$dm,isPredicted = FALSE), all.x=T);

combined2.df = combined.df[combined.df$dm>=1 & combined.df$dm<=8,];
combined2.df = combined2.df[order(combined2.df$isPredicted),]

windows()
plot.new()
plot(combined2.df$fairness, combined2.df$stability, xlab="Unfairness", ylab="Unstability", main="Predicted Fairness vs. Stability", col=(c("red", "blue"))[combined2.df$isPredicted + 1])

windows()
plot.new()
plot(combined2.df$dm, combined2.df$stability, xlab="District Magnitude", ylab="Unstability", col=(c("red", "blue"))[combined2.df$isPredicted + 1])


windows()
plot.new()
plot(combined2.df$dm, combined2.df$fairness, xlab="District Magnitude", ylab="Unfairness", col=(c("red", "blue"))[combined2.df$isPredicted + 1])


windows()
plot.new()
cols = c("red","orange","green","blue","purple");
cols2 = cols[stv.df$wpe];
cond = stv.df$wpe >= 1 & stv.df$wpe <= 5;
plot(stv.df$loosemorehanby[cond], stv.df$enp[cond], col = cols2[cond])

mean(stv.df$loosemorehanby[stv.df$wpe==1])
mean(stv.df$loosemorehanby[stv.df$wpe==2])
mean(stv.df$loosemorehanby[stv.df$wpe==3])
mean(stv.df$loosemorehanby[stv.df$wpe==4])

mean(stv.df$enp[stv.df$wpe==1])
mean(stv.df$enp[stv.df$wpe==2])
mean(stv.df$enp[stv.df$wpe==3])
mean(stv.df$enp[stv.df$wpe==4])

windows()
plot.new()
boxplot(stv.df$enp~stv.df$wpe,xlab="District Magnitude",ylab="ENP")

windows()
plot.new()
boxplot(stv.df$loosemorehanby~stv.df$wpe,xlab="District Magnitude",ylab="Loosemorehanby")




stv2.df <- NULL

for (alpha in (0:100)/100)
{
	stv.temp.df = stv.df;
	stv.temp.df$alpha = alpha;
	
	stv2.df = rbind(stv.temp.df, stv2.df, all.x=T)
}

nrow(stv2.df)

stv2.df = stv2.df[stv2.df$wpe < 5,]

nrow(stv2.df)

stv2.df$bad =
	((stv2.df$loosemorehanby - min(stv2.df$loosemorehanby)) / (max(stv2.df$loosemorehanby)-min(stv2.df$loosemorehanby))) * stv2.df$alpha +
	((stv2.df$enp - min(stv2.df$enp)) / (max(stv2.df$enp)-min(stv2.df$enp))) * (1.0 - stv2.df$alpha);

stv2.df$bad =
	((stv2.df$loosemorehanby - mean(stv2.df$loosemorehanby)) / (sd(stv2.df$loosemorehanby) * stv2.df$alpha +
	((stv2.df$enp - mean(stv2.df$governability)) / (sd(stv2.df$governability))) * (1.0 - stv2.df$alpha);

	
rOrder = order(rnorm(nrow(stv2.df)));

windows()
plot.new()
plot(stv2.df$alpha[rOrder], stv2.df$bad[rOrder], col = (c("red","green","blue","black"))[stv2.df$wpe[rOrder]], xlab="Alpha", ylab="Undesirability")


selectedAlpha = 0.62;

stv3.df = stv2.df[stv2.df$alpha==selectedAlpha,]

windows()
plot.new()
boxplot(stv3.df$bad~stv3.df$wpe, xlab="District Magnitude", ylab="Undesirability", main="Expected undesirability by district magnitude. Alpha=0.62")




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
	
	
	