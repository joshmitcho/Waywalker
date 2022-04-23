#include <vector>
#include <string>
#pragma once
class Map
{
public:
	Map(std::string name);
	~Map();
	std::vector< std::vector<int> > getTerrain();

private:
	std::vector< std::vector<int> > terrain;
};

