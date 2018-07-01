simulations.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/CS380/bin/Debug/out/simulations1.csv", sep = ",", skip = 0, header=T)
simulations.df$X <- NULL
simulations.df[1:5,]

plot(simulations.df)

plot(simulations.df$FPP__governability - simulations.df$MMP_0__governability)

plot(simulations.df$FPP__governability, simulations.df$FPP__loosemorehanby)
plot(simulations.df$MMP_0__governability, simulations.df$MMP_0__loosemorehanby)

hist(simulations.df$FPP__governability)
hist(simulations.df$FPP__governability - simulations.df$MMP_0__governability)
hist(simulations.df$FPP__loosemorehanby - simulations.df$MMP_0__loosemorehanby)

plot(simulations.df$mmp_governability - simulations.df$fpp_governability, simulations.df$mmp_loosemorehanby - simulations.df$fpp_loosemorehanby)

plot(simulations.df$fpp_governability, simulations.df$mmp_governability)

mean(simulations.df$fpp_governability)
mean(simulations.df$mmp_governability)
mean(simulations.df$fpp_loosemorehanby)
mean(simulations.df$mmp_loosemorehanby)

hist(simulations.df$fpp_governability - simulations.df$mmp_governability)
hist(simulations.df$fpp_loosemorehanby- simulations.df$mmp_loosemorehanby)
hist(simulations.df$fpp_gallagher - simulations.df$mmp_gallagher)

hist(simulations.df$mmp_loosemorehanby - simulations.df$mmp_gallagher)
plot(simulations.df$mmp_loosemorehanby, simulations.df$mmp_gallagher)

plot(simulations.df$mmp_loosemorehanby, simulations.df$mmp_governability)

plot(simulations.df$mmp_governability, simulations.df$mmp_loosemorehanby)
plot(simulations.df$fpp_governability, simulations.df$fpp_loosemorehanby)