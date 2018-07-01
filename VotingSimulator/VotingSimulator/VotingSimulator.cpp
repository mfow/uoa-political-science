// VotingSimulator.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "CSVReader.h"

#include "VotingSystem.h"
#include "FPPVotingSystem.h"
#include "PVVotingSystem.h"
#include "STVVotingSystem.h"
#include "MMPVotingSystem.h"
#include "ApprovalVotingSystem.h"
#include "CondorcetVotingSystem.h"
#include "Simulator.h"
#include "VotePredictor.h"
#include "DefaultVotePredictor.h"

#define MAX_FILEVERSION 1000

int processFile(wifstream &in);
VotingSystem* getVotingSystem(String method);
void showUsage();
void showVotingSystems();

int _tmain(int argc, _TCHAR* argv[])
{
	int result = 0;

	if (argc <= 1)
	{
		showUsage();
		return 0;
	}

	String command(argv[1]);

	int argIndex = 2;

	if (command.compare(L"count") == 0)
	{
		// Count voting data.

		wifstream in(argv[2]);
		String s;

		if (!in.is_open())
		{
			wcout << "Failed to open file." << endl;
			result = -1;
		}
		processFile(in);
	} else if (command.compare(L"systems") == 0)
	{
		showVotingSystems();
	} else if (command.compare(L"simulate") == 0)
	{
		// Simulate election.

		wcout << "Loading simulator..." << endl;

		int partyCount = 0;
		int electorateCount = 0;
		int scaleSize = -1;			// Size to scale the results to (in seats). -1 to not scale.

		vector<VotingSystem*>* votingSystems = new vector<VotingSystem*>();
		vector<PreferencePredictor*>* prefPredictors = new vector<PreferencePredictor*>();
		vector<DefaultVotePredictor*>* votePredictors = new vector<DefaultVotePredictor*>();
		vector<vector<double>*>* pollResultList = new vector<vector<double>*>();
		vector<String>* partyNames = new vector<String>();
		vector<int>* seatsPerElectorate = new vector<int>();
		vector<int>* actualResults = new vector<int>();

		wofstream resultStrm;
		bool writeOutput = false;

		while(argc - argIndex > 0)
		{
			String arg = argv[argIndex];
			
			if (arg.compare(L"-s") == 0)
			{
				argIndex++;

				String voteMethod = String(argv[argIndex]);

				wcout << L"Voting method: " << voteMethod << endl;

				votingSystems->push_back(getVotingSystem(voteMethod));
				
				argIndex++;
			} else if (arg.compare(L"-pp") == 0)
			{
				// Preference probabilty
				argIndex++;

				wifstream in(argv[argIndex]);
				
				if (partyCount <= 0)
				{
					wcout << L"Unknown party count. Ensure party list (-pl) option set and is the first option.";
					result = -1;
					break;
				}

				PreferencePredictor* predictor = new PreferencePredictor();

				predictor->Load(in, partyCount);

				prefPredictors->push_back(predictor);

				in.close();

				argIndex++;
			}
			else if (arg.compare(L"-sp") == 0)
			{
				// Spatial probability
				argIndex++;

				wifstream in(argv[argIndex]);
				
				if (partyCount <= 0)
				{
					wcout << L"Unknown party count. Ensure party list (-pl) option set and is the first option.";
					result = -1;
					break;
				}

				DefaultVotePredictor* predictor = new DefaultVotePredictor();

				predictor->Load(in, partyCount);

				electorateCount = predictor->electorateCount;
				votePredictors->push_back(predictor);

				in.close();

				argIndex++;
			}
			else if (arg.compare(L"-pl") == 0)
			{
				// Party List
				
				argIndex++;
				
				wifstream in(argv[argIndex]);

				partyCount = _wtoi(readSingle(in).c_str());

				for (int i = 0; i < partyCount; i++)
				{
					partyNames->push_back(readSingle(in));
				}

				in.close();

				argIndex++;
			}
			else if (arg.compare(L"-poll") == 0)
			{
				// Poll results
				argIndex++;

				if (partyCount <= 0)
				{
					wcout << L"Unknown party count. Ensure party list (-pl) option set and is the first option.";
					result = -1;
					break;
				}

				wifstream in(argv[argIndex]);

				double voteTotal = 0.0;
				double* pollScores = new double[partyCount];

				for (int i = 0; i < partyCount; i++)
				{
					double x = _wtof(readSingle(in).c_str());

					voteTotal += x;
					pollScores[i] = x;
				}

				double voteTotalError = voteTotal - 1.0;

				wcout << L"Poll result error: " << voteTotalError << endl;

				if (abs(voteTotalError) > 0.01)
				{
					wcout << L"WARNING: Large error in poll results (" << (voteTotal - 1.0) << L")" << endl; 
				}

				vector<double>* pollResults = new vector<double>();

				for (int i = 0; i < partyCount; i++)
				{
					pollResults->push_back(pollScores[i] / voteTotal);
				}

				pollResultList->push_back(pollResults);

				delete pollScores;

				in.close();

				argIndex++;
			}
			else if (arg.compare(L"-sc") == 0)
			{
				// Scale results.

				argIndex++;

				arg = argv[argIndex];
				argIndex++;

				if ((arg.compare(L"fixed") == 0) | (arg.compare(L"f") == 0))
				{
					// Scale to a fixed size.
					scaleSize = _wtoi(argv[argIndex]);
					argIndex++;
				}
				else if ((arg.compare(L"actual") == 0) | (arg.compare(L"a") == 0))
				{
					// Scale to the size of the actual results.

					if (actualResults->size() == 0)
					{
						wcout << L"Unknown electorate count. Ensure actual result (-actual) option set before this option.";
						result = -1;
						break;
					}

					scaleSize = 0;
					for (int i = 0; i < actualResults->size(); i++)
						scaleSize += actualResults->at(i);
				}
				else
				{
					wcout << L"Unknown arguments";
					return -1;
				}
			}
			else if (arg.compare(L"-es") == 0)
			{
				// Electorate size
				argIndex++;

				if (electorateCount <= 0)
				{
					wcout << L"Unknown actual result size. Ensure party list (-sp) option set before this option.";
					result = -1;
					break;
				}

				arg = argv[argIndex];
				argIndex++;

				if ((arg.compare(L"fixed") == 0) | (arg.compare(L"f") == 0))
				{
					int x = _wtoi(argv[argIndex]);

					for (int i = 0; i < electorateCount; i++)
					{
						seatsPerElectorate->push_back(x);
					}
				} else if ((arg.compare(L"range") == 0) | (arg.compare(L"r") == 0))
				{
					int x = _wtoi(argv[argIndex]);

					argIndex++;
					
					int c = _wtoi(argv[argIndex]);

					for (int i = 0; i < c; i++)
					{
						seatsPerElectorate->push_back(x);
					}
				} else if ((arg.compare(L"external") == 0) | (arg.compare(L"e") == 0))
				{
					wifstream in(argv[argIndex]);

					for (int i = 0; i < electorateCount; i++)
					{
						int x = _wtoi(readSingle(in).c_str());

						seatsPerElectorate->push_back(x);
					}

					in.close();
				}
				else
				{
					wcout << L"Unknown arguments";
					return -1;
				}

				argIndex++;
			}
			else if (arg.compare(L"-actual") == 0)
			{
				argIndex++;
				wifstream in(argv[argIndex]);

				for (int i = 0; i < partyCount; i++)
				{
					int x = _wtoi(readSingle(in).c_str());

					actualResults->push_back(x);
				}

				in.close();
				argIndex++;
			}
			else if (arg.compare(L"-out") == 0)
			{
				argIndex++;

				resultStrm.open(argv[argIndex]);
				resultStrm.clear();

				writeOutput = true;

				argIndex++;
			}
			else
			{
				wcout << L"Unknown argument at index " << argIndex << " ";
				wcout << arg.c_str();
				wcout << endl;
				result = -1;
				break;
			}
		}

		vector<Simulator*>* simulators = new vector<Simulator*>();

		int simulatorCount =
			votingSystems->size() *
			prefPredictors->size() *
			votePredictors->size() *
			pollResultList->size();

		if (simulatorCount == 0)
		{
			result = -1;
			wcout << "Incomplete options. Zero simulators generated.";
		}

		if (seatsPerElectorate->size() < electorateCount)
		{
			seatsPerElectorate->clear();

			wcout << L"Using default electorate size." << endl;
			// Load default electorate size.
			for (int i = 0; i < electorateCount; i++)
			{
				seatsPerElectorate->push_back(1);
			}
		}

		if (seatsPerElectorate->size() > electorateCount)
		{
			wcout << L"Warning: Excess electorate seat information will be ignored." << endl;
		}

		for (int a = 0; a < votingSystems->size(); a++)
		{
			for (int b = 0; b < prefPredictors->size(); b++)
			{
				for (int c = 0; c < votePredictors->size(); c++)
				{
					for (int d = 0; d < pollResultList->size(); d++)
					{
						Simulator* sim = new Simulator();

						sim->partyCount = partyCount;
						sim->electorateCount = electorateCount;
						sim->partyNames = partyNames;

						sim->votingSystem = votingSystems->at(a);
						sim->prefPredictor = prefPredictors->at(b);
						sim->predictor = votePredictors->at(c);
						sim->electorateSeats = seatsPerElectorate;

						delete sim->pollResults;
						sim->pollResults = pollResultList->at(d);
						
						simulators->push_back(sim);
					}
				}
			}
		}

		if (result == 0)
		{
			wcout << "Simulation loaded." << endl;

			vector<vector<int>*>* simulationResults = new vector<vector<int>*>();
			vector<int>* simulationResultTotals = new vector<int>();

			for (int i = 0; i < simulators->size(); i++)
			{
				wcout << "Running simulation " << (i + 1) << L" of " << simulators->size() << L" ...";

				vector<int>* simulationResult = simulators->at(i)->Simulate();
				simulationResults->push_back(simulationResult);
				
				int simulationResultTotal = 0;
				for (int j = 0; j < simulationResult->size(); j++)
					simulationResultTotal += simulationResult->at(j);

				simulationResultTotals->push_back(simulationResultTotal);

				wcout << "Complete" << endl;
			}

			for (int i = 0; i < simulationResults->at(0)->size(); i++)
			{
				wcout << simulators->at(0)->partyNames->at(i);

				if (writeOutput)
					resultStrm << simulators->at(0)->partyNames->at(i) << L",";

				for (int j = 0; j < simulators->size(); j++)
				{
					int x = simulationResults->at(j)->at(i);

					double y = (scaleSize == -1) ? (double)x : (double)x * (double)scaleSize / (double)simulationResultTotals->at(j);

					wcout << L" " << y;

					if (writeOutput)
						resultStrm << y << L",";
				}

				if (actualResults->size() == partyCount)
				{
					int x = actualResults->at(i);

					wcout << L" " << x;

					if (writeOutput)
						resultStrm << x << L",";
				}

				wcout << endl;
				if (writeOutput)
						resultStrm << endl;
			}

			delete simulationResultTotals;
			delete votingSystems;
			delete prefPredictors;
			delete votePredictors;
			delete pollResultList;
			delete partyNames;
			delete simulationResults;
			delete seatsPerElectorate;
			delete actualResults;
		}

		for (int i = 0; i < simulators->size(); i++)
		{
			delete simulators->at(i);
		}

		delete simulators;

		if (writeOutput)
			resultStrm.close();
	} else {
		wcout << L"Unknown command: ";
		wcout << command.c_str();
		wcout << endl;

		result = -1;
	}

	// Stop the program from finishing early...
#if DEBUG
	getchar();
#endif

	return result;
}

int processFile(wifstream &in)
{
	if (readSingle(in).compare(L"Voting Preferences") != 0)
	{
		return -1;
	}

	map<String, String>* fileMetaData = readHeaderTuples(in);

	int version = _wtoi(fileMetaData->at(L"Version").c_str());
	
	if (version > MAX_FILEVERSION)
	{
		wcout << L"File Version " << version << L" is above the maximum version " << MAX_FILEVERSION << L"." << endl;
		return -1;
	}

	String method = fileMetaData->at(L"Method");
	transform(method.begin(), method.end(), method.begin(), tolower);

	VotingSystem* votingSys = getVotingSystem(method);

	if (votingSys == NULL)
	{
		wcout << L"Unknown voting method." << endl;
		return -1;
	}

	map<String, String>* customMetaData = readHeaderTuples(in);

	int partyCount = _wtoi(readSingle(in).c_str());

	if (votingSys->Load(in, partyCount, customMetaData) != 0)
	{
		wcout << "Error processing voting data." << endl;
		return -1;
	}

	delete customMetaData;

	if (votingSys->PrintResults() != 0)
	{
		wcout << "Error printing voting results." << endl;
		return -1;
	}

	return 0;
}

VotingSystem* getVotingSystem(String method)
{
	if (method.compare(L"fpp") == 0)
	{
		return new FPPVotingSystem(FPPOptions::FirstPastThePost);
	}

	if (method.compare(L"approval") == 0)
	{
		return new ApprovalVotingSystem(ApprovalOptions::Approval);
	}

	if (method.compare(L"borda") == 0)
	{
		return new ApprovalVotingSystem(ApprovalOptions::Borda);
	}

	if (method.compare(L"antiplurality") == 0)
	{
		return new FPPVotingSystem(FPPOptions::AntiPlurality);
	}

	if (method.compare(L"pv") == 0)
	{
		return new PVVotingSystem(PVOptions::PreferentialVoting);
	}

	if (method.compare(L"bucklin") == 0)
	{
		return new PVVotingSystem(PVOptions::Bucklin);
	}
	
	if (method.compare(L"stv.instant") == 0)
	{
		return new STVVotingSystem(STVTransferMethod::InstantTransfer);
	}

	if (method.compare(L"stv.fractional") == 0)
	{
		return new STVVotingSystem(STVTransferMethod::FractionalTransfer);
	}

	if (method.compare(L"stv.random") == 0)
	{
		return new STVVotingSystem(STVTransferMethod::RandomTransfer);
	}

	if (method.compare(L"stv.stack") == 0)
	{
		return new STVVotingSystem(STVTransferMethod::RandomTransfer);
	}

	if (method.compare(L"stv.queue") == 0)
	{
		return new STVVotingSystem(STVTransferMethod::QueueTransfer);
	}

	if (method.compare(L"mmp.overhang") == 0)
	{
		return new MMPVotingSystem(MMPOptions::AllowOverhang);
	}

	if (method.compare(L"mmp.nonaward") == 0)
	{
		return new MMPVotingSystem(MMPOptions::NonAward);
	}

	if (method.compare(L"mmp.nooverhang") == 0)
	{
		return new MMPVotingSystem(MMPOptions::NoOverhang);
	}

	if (method.compare(L"sm") == 0)
	{
		return new MMPVotingSystem(MMPOptions::SupplementaryMember);
	}

	if (method.compare(L"proportional") == 0)
	{
		return new MMPVotingSystem(MMPOptions::Proportional);
	}

	if (method.compare(L"copelands") == 0)
	{
		return new CondorcetVotingSystem(CondorcetOptions::Copelands);
	}

	if (method.compare(L"minimax") == 0)
	{
		return new CondorcetVotingSystem(CondorcetOptions::Minimax);
	}

	return NULL;
}

void showUsage()
{
	wcout
		<< L"Usage:" << endl
		<< L"VotingSimulator count votes.pref" << endl
		<< L"Counts the votes in the given file" << endl << endl
		<< L"VotingSimulator simulate -s -pl -pp -sp -poll [-s]" << endl
		<< L"Simulates the given election" << endl << endl
		<< L"VotingSimulator systems" << endl
		<< L"Shows a list of supported voting methods." << endl << endl
		<< L"Parameters:" << endl
		<< L"votes.pref						A file which represents the votes in a given election." << endl
		<< L"-s								Election system. Followed by the code for the specified system." << endl
		<< L"-pl							Party List. Followed by the filename of a csv file of party names." << endl
		<< L"-pp							Preference Probability. Followed by the filename of a preference probability file." << endl
		<< L"-sp							Spatial Probability. Followed by the filename of a spatial distribution probability file." << endl
		<< L"-poll							Poll results. Followed by the filename of national level poll results." << endl
		<< L"-es							(Optional, defaults to one) Electorate size. Either followed by one of:" << endl
		<< L"	fixed		or f			Followed by an integer size. Fixed size per electorate for all electorates." << endl
		<< L"	range		or r			Followed by an integer size and integer count. Fixed size per electorate for a number of electorates." << endl
		<< L"	external	or e			Followed by a filename of a csv file which contains electorate sizes." << endl
		<< L"	Multiple instances of the -es argument may be selected." << endl << endl
		<< L"-sc							(Optional, defaults to no scaling) Scale total number of seats:" << endl
		<< L"	fixed		or f			Followed by an integer size. Fixed number of seats." << endl
		<< L"	actual		or a			Sets the fixed number of seats to be equal to the sum of seats in results given by the -actual argument." << endl
		<< L"Multiple instances of the -s -pp -sp -poll options may be selected and a simulation will run for each possible "
		<< L"combination of arguments." << endl;
}

void showVotingSystems()
{
	wcout
		<< L"fpp" << endl
		<< L"antiplurality" << endl
		<< L"pv" << endl
		<< L"proportional" << endl
		<< L"sm" << endl
		<< L"stv.instant" << endl
		<< L"stv.fractional" << endl
		<< L"copelands" << endl
		<< L"minimax" << endl ;
}