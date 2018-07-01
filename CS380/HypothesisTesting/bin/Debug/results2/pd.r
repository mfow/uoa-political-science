#results.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results2/pd_spatial2_results.csv", sep = ",", skip = 0, header=T)
results.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results4/spatial2stv.csv", sep = ",", skip = 0, header=T)
#results.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results5/urnstv.csv", sep = ",", skip = 0, header=T)
results.df$X <- NULL
results.df[1:5,]

results.df$fairness = results.df$gallagher;
#results.df$stability = results.df$enp-1;
results.df$stability = (1/results.df$governability)-1;

windows()
plot.new()
results.df$col1 = "blue"
results.df$col1[results.df$dm<10] = "green"
results.df$col1[results.df$dm==1] = "red"
rOrder = order(rnorm(nrow(results.df)))
plot(results.df$fairness[rOrder], results.df$stability[rOrder], xlab="Unfairness", ylab="Instability",col=results.df$col1[rOrder]) 
results.df$col1 <- NULL

#Fit fairness
windows()
plot.new()
plot(results.df$dm, results.df$fairness) 

windows()
plot.new()
plot(results.df$dm, log(1/results.df$fairness))

fairness.fit = lm((1/fairness)~I(dm)+I(dm^2), data=results.df)

summary(fairness.fit)

windows()
plot.new()
plot(predict(fairness.fit,data=results.df), resid(fairness.fit,data=results.df))

#Fit stability
windows()
plot.new()
plot(results.df$dm, results.df$stability)

stability.fit = lm(stability~I(dm)+I(log(dm))+I(dm^2)+I(dm^3), data=results.df)

summary(stability.fit)

windows()
plot.new()
plot(predict(stability.fit,data=results.df), resid(stability.fit,data=results.df))



dummy.df = data.frame(dm=(10:1200)/10)
dummy.df$fairness = 1/(predict(fairness.fit, dummy.df))
dummy.df$stability = predict(stability.fit, dummy.df)
dummy.df$isPredicted = TRUE;
dummy.df$type=1

combined.df = dummy.df;
combined.df = rbind(dummy.df, data.frame(fairness=results.df$fairness, stability=results.df$stability, dm=results.df$dm,isPredicted = FALSE), all.x=T);

combined2.df = combined.df[combined.df$dm>=1 & combined.df$dm<=8,];
combined2.df = combined2.df[order(combined2.df$isPredicted),]

windows()
plot.new()
plot(combined2.df$dm, combined2.df$stability, xlab="District Magnitude", ylab="Unstability", col=(c("red", "blue"))[combined2.df$isPredicted + 1])

windows()
plot.new()
plot(combined2.df$dm, combined2.df$fairness, xlab="District Magnitude", ylab="Unfairness", col=(c("red", "blue"))[combined2.df$isPredicted + 1])

windows()
plot.new()
plot(combined2.df$fairness, combined2.df$stability, xlab="Unfairness", ylab="Unstability", main="Predicted Fairness vs. Stability", col=(c("red", "blue"))[combined2.df$isPredicted + 1])


dummy.df$col1 = "blue"
dummy.df$col1[dummy.df$dm<10] = "green"
dummy.df$col1[dummy.df$dm==1] = "red"

windows()
plot.new()
plot(dummy.df$fairness, dummy.df$stability, xlab="Unfairness", ylab="Unstability", main="Predicted Fairness vs. Stability", col=dummy.df$col1)

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

actualmeans.df$col1 = "blue"
actualmeans.df$col1[actualmeans.df$dm<10] = "green"
actualmeans.df$col1[actualmeans.df$dm==1] = "red"

windows()
plot.new()
plot(actualmeans.df$fairness, actualmeans.df$stability, xlab="Unfairness", ylab="Unstability", main="Actual Mean Fairness vs. Stability", col=actualmeans.df$col1)

# 2nd regression attempt. Based on means per DM.
fairness2.fit = lm((1/fairness)~I(dm)+I(dm^2)+I(log(dm)), data=actualmeans.df)
stability2.fit = lm(stability~I(dm)+I(log(dm))+I(dm^2), data=actualmeans.df)

summary(fairness2.fit)
summary(stability2.fit)

dummy2.df = data.frame(dm=(10:1200)/10)
dummy2.df$fairness = 1/(predict(fairness2.fit, dummy2.df))
dummy2.df$stability = predict(stability2.fit, dummy2.df)
dummy2.df$type=2
dummy2.df$isPredicted=TRUE

windows()
plot.new()
plot(dummy2.df$fairness, dummy2.df$stability, xlab="Unfairness", ylab="Unstability", main="Predicted[2] Fairness vs. Stability")


combined3.df = dummy2.df
#combined3.df = rbind(combined3.df, dummy.df, all.x=T)
combined3.df = combined3.df[order(rnorm(nrow(combined3.df))),]
combined3.df = rbind(combined3.df, actualmeans.df, all.x=T)
combined3.df$color = (c("black", "red", "green"))[combined3.df$type+1]

#reorder
#combined3.df = combined3.df[order(rnorm(nrow(combined3.df))),]
plot(combined3.df$fairness, combined3.df$stability, xlab="Unfairness", ylab="Unstability", main="Combined Fairness vs. Stability", col=combined3.df$color)


plot(combined3.df$dm, combined3.df$stability, xlab="District Magnitude", ylab="Unstability", main="District Magnitude vs. Stability", col=combined3.df$color)

