#include "StdAfx.h"
#include "FPPVotingSystem.h"
#include "CSVReader.h"

FPPVotingSystem::FPPVotingSystem(FPPOptions options)
{
	this->options = options;
}


FPPVotingSystem::~FPPVotingSystem(void)
{

}

int FPPVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData)
{
	vector<String>* partyHeaders = readCSVLine(in);

	for (int h = 0; h < partyCount; h++)
	{
		vector<String>* partyData = readCSVLine(in);

		delete partyData;
	}

	delete partyHeaders;

	this->electorateCount = _wtoi(readSingle(in).c_str());
	this->electorates = new FPPElectorate[FPPVotingSystem::electorateCount];

	for (int i = 0; i < FPPVotingSystem::electorateCount; i++)
	{
		FPPElectorate electorate;
		electorate.candidateCount = _wtoi(readSingle(in).c_str());
		electorate.candidates = new FPPCandidate[electorate.candidateCount];

		electorate.properties = readHeaderTuples(in);

		vector<String>* candidateHeaders = readCSVLine(in);
		
		int partyHeaderIndex = getIndexOf(candidateHeaders, L"Party");

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			FPPCandidate candidate;

			vector<String>* candidateData = readCSVLine(in);

			candidate.properties = getHeaderTuples(candidateHeaders, candidateData);
			candidate.partyIndex = _wtoi(candidateData->at(partyHeaderIndex).c_str());
			candidate.votes = 0;

			electorate.candidates[j] = candidate;

			delete candidateData;
		}

		delete candidateHeaders;

		this->electorates[i] = electorate;

		int numberOfVotesInElectorate = _wtoi(readSingle(in).c_str());

		vector<String>* voteHeaders = readCSVLine(in);
		int voteHeaderIndex = getIndexOf(voteHeaders, L"Vote");

		for (int k = 0; k < numberOfVotesInElectorate; k++)
		{
			vector<String>* voteData = readCSVLine(in);
			int voteIndex = _wtoi(voteData->at(voteHeaderIndex).c_str());

			FPPCandidate cand = electorate.candidates[voteIndex];
			cand.votes++;
			electorate.candidates[voteIndex] = cand;

			delete voteData;
		}

		delete voteHeaders;
	}

	return 0;
}

int FPPVotingSystem::PrintResults()
{
	wcout << "Election type: FPP" << endl;
	wcout << "Electorate count: " << this->electorateCount << endl;

	for (int i = 0; i < this->electorateCount; i++)
	{
		FPPElectorate electorate = this->electorates[i];

		wcout << "Electorate " << i << endl;
		
		int winningIndex = 0;
		int winningVoteCount = 0;

		switch (this->options)
		{
			case FPPOptions::FirstPastThePost:
				winningVoteCount = 0;
				break;
			case FPPOptions::AntiPlurality:
				winningVoteCount = INT_MAX;
				break;
			default:
				wcout << "Unknown options." << endl;
				return -1;
		}

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			int candidateVotes = electorate.candidates[j].votes;

			switch (this->options)
			{
				case FPPOptions::FirstPastThePost:
					if (candidateVotes > winningVoteCount)
					{
						winningIndex = j;
						winningVoteCount = candidateVotes;
					}
					break;
				case FPPOptions::AntiPlurality:
					if (candidateVotes < winningVoteCount)
					{
						winningIndex = j;
						winningVoteCount = candidateVotes;
					}
					break;
				default:
					break;
			}
		}

		wcout << L"Winner " << electorate.candidates[winningIndex].properties->at(L"Name") << " " << winningVoteCount << L" votes" << endl << endl;
	}

	return 0;
}

vector<int>* FPPVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info)
{
	vector<PreferenceOrder*>* prefOrders = info.prefOrders;
	int partyCount = info.partyCount;

	double* scores = new double[partyCount];

	for (int i = 0; i < partyCount; i++)
		scores[i] = 0.0;

	for (int i = 0; i < prefOrders->size(); i++)
	{
		PreferenceOrder* pref = prefOrders->at(i);
		
		switch (this->options)
		{
			case FPPOptions::FirstPastThePost:
				scores[pref->order->at(0)] += pref->weight;
				break;
			case FPPOptions::AntiPlurality:
				scores[pref->order->at(pref->order->size() - 1)] -= pref->weight;
				break;
			default:
				wcout << "Unknown options." << endl;
				return NULL;
		}
	}

	double maxValue = scores[0];
	
	for (int i = 0; i < partyCount; i++)
		maxValue = scores[i] > maxValue ? scores[i] : maxValue;

	vector<int>* result = new vector<int>();

	for (int i = 0; i < partyCount; i++)
		if (scores[i] >= maxValue)
			result->push_back(i);

	delete scores;

	return result;
}