#include "stdafx.h"
#include "Map.h"
#include <iostream>
#include <fstream>
using namespace std;


Map::Map(string name)
{
	int y = 0;
	string line;
	ifstream myfile("Resources\\Maps\\" + name + ".txt");
	if (myfile.is_open())
	{
		while (getline(myfile, line))
		{
			vector<int> row;
			terrain.push_back(row);

			for (int x = 0; x < line.size(); x++) {
				if (line[x] == ' ') {
					terrain[y].push_back(1);
				}
				else if (line[x] == 'X') {
					terrain[y].push_back(0);
				}
				else if (line[x] == '~') {
					terrain[y].push_back(2);
				}
			}
			y++;
		}
		myfile.close();
	}

	else cout << "Unable to open file";
}


Map::~Map()
{
}

vector< vector<int> > Map::getTerrain()
{
	return terrain;
}

