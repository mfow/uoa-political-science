#include "StdAfx.h"
#include "STVVotingSystem.h"
#include "CSVReader.h"

STVVotingSystem::STVVotingSystem(STVTransferMethod transferMethod)
{
	this->transferMethod = transferMethod;
}


STVVotingSystem::~STVVotingSystem(void)
{
}

int STVVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData)
{
	vector<String>* partyHeaders = readCSVLine(in);

	for (int h = 0; h < partyCount; h++)
	{
		vector<String>* partyData = readCSVLine(in);

		delete partyData;
	}

	delete partyHeaders;

	this->electorateCount = _wtoi(readSingle(in).c_str());
	this->electorates = new STVElectorate[STVVotingSystem::electorateCount];

	for (int i = 0; i < STVVotingSystem::electorateCount; i++)
	{
		STVElectorate electorate;
		electorate.candidateCount = _wtoi(readSingle(in).c_str());
		electorate.candidates = new STVCandidate[electorate.candidateCount];
		electorate.votes = new vector<STVVote>;
		electorate.properties = readHeaderTuples(in);
		electorate.seatCount = _wtoi(electorate.properties->at(L"SeatCount").c_str());

		vector<String>* candidateHeaders = readCSVLine(in);

		int partyHeaderIndex = getIndexOf(candidateHeaders, L"Party");

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			STVCandidate candidate;

			vector<String>* candidateData = readCSVLine(in);

			candidate.properties = getHeaderTuples(candidateHeaders, candidateData);
			candidate.partyIndex = _wtoi(candidateData->at(partyHeaderIndex).c_str());
			candidate.votes = 0;
			candidate.isExcluded = false;
			candidate.isElected = false;

			electorate.candidates[j] = candidate;

			delete candidateData;
		}

		delete candidateHeaders;

		int numberOfVotesInElectorate = _wtoi(readSingle(in).c_str());

		vector<String>* voteHeaders = readCSVLine(in);
		int voteHeaderIndex = getIndexOf(voteHeaders, L"Vote");

		for (int k = 0; k < numberOfVotesInElectorate; k++)
		{
			vector<String>* voteData = readCSVLine(in);

			STVVote vote;
			vote.votes = new int[electorate.candidateCount];
			vote.weight = 1;

			for (int m = 0; m < electorate.candidateCount; m++)
			{
				int voteIndex = _wtoi(voteData->at(voteHeaderIndex + m).c_str());

				vote.votes[m] = voteIndex;
			}

			electorate.votes->push_back(vote);

			delete voteData;
		}

		this->electorates[i] = electorate;

		delete voteHeaders;
	}

	return 0;
}

int STVVotingSystem::PrintResults()
{
	wcout << L"Election type: STV" << endl;
	wcout << L"Electorate count: " << this->electorateCount << endl;

	for (int i = 0; i < this->electorateCount; i++)
	{
		STVElectorate electorate = this->electorates[i];

		wcout << L"Electorate " << i << endl;
		
		vector<int>* winners = this->calculateElectorateWinners(electorate);

		for (int j = 0; j < winners->size(); j++)
		{
			wcout << L"Winner " << electorate.candidates[j].properties->at(L"Name") << endl;
		}

		delete winners;

		wcout << endl;
	}

	return 0;
}

vector<int>* STVVotingSystem::calculateElectorateWinners(STVElectorate electorate)
{
	vector<int>* winners = new vector<int>();

	double totalWeight = 0.0;

	for (int i = 0; i < electorate.votes->size(); i++)
		totalWeight += electorate.votes->at(i).weight;

	electorate.quota = (totalWeight / (electorate.seatCount + 1.0)) + 0.0;

	int majorityCutoff = electorate.votes->size() / 2;

	int electedCount = 0;

	// Loop for the number of rounds. (Max number of rounds = number of candidates)
	while(electorate.seatCount > electedCount)
	{
		// Reset vote count for electorate.
		for (int j = 0; j < electorate.candidateCount; j++)
		{
			electorate.candidates[j].votes = 0;
		}

		int losingIndex = 0;
		double losingVoteCount = (double)INT_MAX;

		// Count this round's votes.
		for (int j = 0; j < electorate.votes->size(); j++)
		{
			STVVote vote = electorate.votes->at(j);

			int voteIndex = -1;

			for (int m = 0; m < electorate.candidateCount; m++)
			{
				if (vote.votes[m] == -1)
				{
					// All this voter's candidates have been excluded
					break;
				}

				if (!electorate.candidates[vote.votes[m]].isExcluded)
				{
					voteIndex = vote.votes[m];
					break;
				}
			}

			// Check if the current vote is valid in this round.
			if (voteIndex >= 0)
			{
				// If so, count it.
				electorate.candidates[voteIndex].votes += vote.weight;
				vote.lastVote = voteIndex;
				electorate.votes->data()[j] = vote;
			}
		}

		int thisRoundWinnerCount = 0;

		// Calculate winners for this round.
		for (int j = 0; j < electorate.candidateCount; j++)
		{
			if ((electorate.candidates[j].isExcluded == false) && (electorate.candidates[j].isElected == false))
			{
				if (electorate.candidates[j].votes >= electorate.quota)
				{
					electorate.candidates[j].isElected = true;
					thisRoundWinnerCount++;
					electedCount++;
					winners->push_back(j);

					if (this->transferMethod == STVTransferMethod::FractionalTransfer)
					{
						// Reduce value of votes.

						double surplusMultiplier = (electorate.candidates[j].votes / (double)electorate.quota) - 1.0;

						for (int j = 0; j < electorate.votes->size(); j++)
						{
							STVVote vote = electorate.votes->at(j);

							if (vote.lastVote == j)
							{
								// This candidate voted for this candidate in this round.

								vote.weight *= surplusMultiplier;
								electorate.votes->data()[j] = vote;
							}
						}
					}
				}

				if (electorate.candidates[j].votes < losingVoteCount)
				{
					losingIndex = j;
					losingVoteCount = electorate.candidates[j].votes;
				}
			}
		}

		if (thisRoundWinnerCount == 0)
		{
			// Nobody won in this round...eliminate the candidate with the least votes.

			electorate.candidates[losingIndex].isExcluded = true;
		}
		else
		{
			switch (this->transferMethod)
			{
				case STVTransferMethod::InstantTransfer:
				case STVTransferMethod::FractionalTransfer:
					// Nothing required. here
					break;
				default:
					wcout << L"Unknown or non-implemented transfer method." << endl;
					return NULL;
			}
		}

		electorate.candidates[losingIndex].isExcluded = true;
	}

	return winners;
}

vector<int>* STVVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info)
{
	vector<PreferenceOrder*>* prefOrders = info.prefOrders;
	int partyCount = info.partyCount;

	STVElectorate electorate;

	electorate.seatCount = info.expectedWinnerCount;
	electorate.candidateCount = partyCount;
	electorate.candidates = new STVCandidate[partyCount];
	electorate.votes = new vector<STVVote>();

	for (int i = 0; i < partyCount; i++)
	{
		STVCandidate cand;

		cand.isElected = false;
		cand.isExcluded = false;
		cand.partyIndex = i;
		cand.votes = 0;
		
		electorate.candidates[i] = cand;
	}

	for (int i = 0; i < prefOrders->size(); i++)
	{
		PreferenceOrder* pref = prefOrders->at(i);
		STVVote vote;

		vote.weight = pref->weight;
		vote.votes = new int[partyCount];

		for (int j = 0; j < partyCount; j++)
		{
			vote.votes[j] = pref->order->at(j);
		}

		electorate.votes->push_back(vote);
	}

	vector<int>* result = this->calculateElectorateWinners(electorate);


	for (int i = 0; i < electorate.votes->size(); i++)
	{
		delete electorate.votes->at(i).votes;
	}

	delete electorate.votes;
	delete electorate.candidates;

	return result;
}