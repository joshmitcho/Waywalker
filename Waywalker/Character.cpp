#include "stdafx.h"
#include "Character.h"


Character::Character(string name, string race, int lvl)
{
	this->name = name;
	this->race = race;
	this->level = lvl;
	setWalkSpeed();
}


Character::~Character()
{
}

string Character::getName()
{
	return name;
}

int Character::getWalkSpeed()
{
	return walkSpeed;
}

void Character::setStats(int hp, int str, int dex, int con, int intell, int wis, int cha) {
	this->hp = hp;
	strength = str;
	dexterity = dex;
	constitution = con;
	intelligence = intell;
	wisdom = wis;
	charisma = cha;
}

void Character::setWalkSpeed()
{
	if (race == "Dwarf" || race == "Halfling") {
		walkSpeed = 20;
	}
	else {
		walkSpeed = 30;
	}
}
