#include "StdAfx.h"
#include "PVVotingSystem.h"
#include "CSVReader.h"

PVVotingSystem::PVVotingSystem(PVOptions options)
{
	this->options = options;
}


PVVotingSystem::~PVVotingSystem(void)
{

}

int PVVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData)
{
	vector<String>* partyHeaders = readCSVLine(in);

	for (int h = 0; h < partyCount; h++)
	{
		vector<String>* partyData = readCSVLine(in);

		delete partyData;
	}

	delete partyHeaders;

	this->electorateCount = _wtoi(readSingle(in).c_str());
	this->electorates = new PVElectorate[PVVotingSystem::electorateCount];

	for (int i = 0; i < PVVotingSystem::electorateCount; i++)
	{
		PVElectorate electorate;
		electorate.candidateCount = _wtoi(readSingle(in).c_str());
		electorate.candidates = new PVCandidate[electorate.candidateCount];
		electorate.votes = new vector<PVVote>;

		electorate.properties = readHeaderTuples(in);

		vector<String>* candidateHeaders = readCSVLine(in);

		int partyHeaderIndex = getIndexOf(candidateHeaders, L"Party");

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			PVCandidate candidate;

			vector<String>* candidateData = readCSVLine(in);

			candidate.properties = getHeaderTuples(candidateHeaders, candidateData);
			candidate.partyIndex = _wtoi(candidateData->at(partyHeaderIndex).c_str());
			candidate.votes = 0;
			candidate.isExcluded = false;

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

			PVVote vote;
			vote.votes = new int[electorate.candidateCount];

			for (int m = 0; m < electorate.candidateCount; m++)
			{
				int voteIndex = _wtoi(voteData->at(voteHeaderIndex + m).c_str());

				vote.votes[m] = voteIndex;
			}

			electorate.votes->push_back(vote);

			delete voteData;
		}

		delete voteHeaders;
	}

	return 0;
}

int PVVotingSystem::PrintResults()
{
	wcout << L"Election type: PV" << endl;
	wcout << L"Electorate count: " << this->electorateCount << endl;

	for (int i = 0; i < this->electorateCount; i++)
	{
		PVElectorate electorate = this->electorates[i];

		wcout << L"Electorate " << i << endl;
		
		int majorityCutoff = electorate.votes->size() / 2;

		int winningIndex ;
		int winningVoteCount;

		// Loop for the number of rounds. (Max number of rounds = number of candidates)
		for (int k = 0; k < electorate.candidateCount; k++)
		{
			int losingIndex = 0;
			int losingVoteCount = INT_MAX;

			winningIndex = 0;
			winningVoteCount = 0;

			// Reset vote count for electorate.
			for (int j = 0; j < electorate.candidateCount; j++)
			{
				electorate.candidates[j].votes = 0;
			}

			// Count this round's votes.
			for (int j = 0; j < electorate.votes->size(); j++)
			{
				PVVote vote = electorate.votes->at(j);
				int voteIndex = -1;

				switch (this->options)
				{
					case PVOptions::PreferentialVoting:
						for (int m = 0; m < electorate.candidateCount; m++)
						{
							// All this voter's candidates have been exluded.
							if (vote.votes[m] == -1)
							{
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
							electorate.candidates[voteIndex].votes++;
						}
						break;
					case PVOptions::Bucklin:
						for (int j2 = 0; j2 < j + 1; j++)
						{
							voteIndex = vote.votes[j2];

							// Check if the current vote is valid in this round.
							if (voteIndex >= 0)
							{
								// If so, count it.
								electorate.candidates[voteIndex].votes++;
							}
						}
						break;
				}
			}

			// Calculate winner and loser for this round.
			for (int j = 0; j < electorate.candidateCount; j++)
			{
				if (electorate.candidates[j].isExcluded == false)
				{
					int candidateVotes = electorate.candidates[j].votes;

					if (candidateVotes > winningVoteCount)
					{
						winningIndex = j;
						winningVoteCount = candidateVotes;
					}

					if (candidateVotes < losingVoteCount)
					{
						losingIndex = j;
						losingVoteCount = candidateVotes;
					}
				}
			}

			// Is this round's winner a majority?
			if (winningVoteCount >= majorityCutoff)
			{
				break;
			}

			wcout << L"Round " << (k + 1) <<
				L" Winner " << electorate.candidates[winningIndex].properties->at(L"Name") << " " << winningVoteCount << L" votes" <<
				L" Loser " << electorate.candidates[losingIndex].properties->at(L"Name") << " " << losingVoteCount << L" votes" <<
				endl;

			if (this->options == PVOptions::PreferentialVoting)
			{
				electorate.candidates[losingIndex].isExcluded = true;
			}
		}

		wcout << L"Winner " << electorate.candidates[winningIndex].properties->at(L"Name") << " " << winningVoteCount << L" votes" << endl << endl;
	}

	return 0;
}

vector<int>* PVVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info)
{
	vector<PreferenceOrder*>* prefOrders = info.prefOrders;
	int partyCount = info.partyCount;

	double winningVoteCount = 0.0;
	double* voteCount = new double[partyCount];

	for (int j = 0; j < partyCount; j++)
	{
		voteCount[j] = 0.0;
	}

	// Loop for the number of rounds. (Max number of rounds = number of candidates)
	for (int k = 0; k < partyCount; k++)
	{
		int losingIndex = 0;
		double losingVoteCount = INT_MAX;

		winningVoteCount = 0.0;

		// Reset vote count for electorate.
		for (int j = 0; j < partyCount; j++)
		{
			voteCount[j] = voteCount[j] < 0.0 ? -1 : 0.0;		// -1 means "excluded"
		}

		// Count this round's votes.
		for (int j = 0; j < prefOrders->size(); j++)
		{
			PreferenceOrder* vote = prefOrders->at(j);
			int voteIndex = -1;

			switch (this->options)
			{
				case PVOptions::PreferentialVoting:
					for (int m = 0; m < partyCount; m++)
					{
						if (vote->order->at(m) == -1)
						{
							// All this voter's candidates have been exluded.
							break;
						}

						if (!(voteCount[vote->order->at(m)] < 0.0))		// Check if candidate is excluded
						{
							voteIndex = vote->order->at(m);
							break;
						}
					}

					// Check if the current vote is valid in this round.
					if (voteIndex >= 0)
					{
						// If so, count it.
						voteCount[voteIndex] += vote->weight;
					}
					break;
				case PVOptions::Bucklin:
					for (int j2 = 0; j2 < j + 1; j++)
					{
						voteIndex = vote->order->at(j2);

						// Check if the current vote is valid in this round.
						if (voteIndex >= 0)
						{
							// If so, count it.
							voteCount[voteIndex] += vote->weight;
						}
					}
					break;
			}
		}

		// Calculate winner and loser for this round.
		for (int j = 0; j < partyCount; j++)
		{
			if ((voteCount[j] < 0.0) == false)
			{
				double candidateVotes = voteCount[j];

				if (candidateVotes > winningVoteCount)
				{
					winningVoteCount = candidateVotes;
				}

				if (candidateVotes < losingVoteCount)
				{
					losingIndex = j;
					losingVoteCount = candidateVotes;
				}
			}
		}

		// Is this round's winner a majority?
		if (winningVoteCount >= 0.5)
		{
			break;
		}

		if (this->options == PVOptions::PreferentialVoting)
		{
			voteCount[losingIndex] = -1.0;
		}
	}	

	vector<int>* winners = new vector<int>();

	for (int i = 0; i < partyCount; i++)
		if (voteCount[i] >= winningVoteCount)
			winners->push_back(i);

	delete voteCount;

	return winners;
}