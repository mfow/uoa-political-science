#pragma once
#include "stdafx.h"
#include "votingsystem.h"

// Although this class is called FPPVotingSystem, it includes a special case for antiplurality.

enum FPPOptions
{
	FirstPastThePost,
	AntiPlurality
};

struct FPPCandidate
{
	int partyIndex;
	int votes;
	map<String, String>* properties;
};

struct FPPElectorate
{
	int candidateCount;
	FPPCandidate* candidates;
	map<String, String>* properties;
};

class FPPVotingSystem :
	public VotingSystem
{
public:
	FPPVotingSystem(FPPOptions options);
	~FPPVotingSystem(void);
	int FPPVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData);
	int FPPVotingSystem::PrintResults();
	vector<int>* FPPVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info);

private:
	int electorateCount;
	FPPElectorate* electorates;
	FPPOptions options;
};

