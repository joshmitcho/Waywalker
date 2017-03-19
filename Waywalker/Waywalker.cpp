// Waywalker.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <SFML/Graphics.hpp>
#include <SFML/Audio.hpp>
#include "Character.h"
#include "Map.h"

using namespace std;

int main()
{
	// Make a window that is 1280 by 720 pixels
	// And has the title "Waywalker"
	sf::RenderWindow window(sf::VideoMode(1280, 720), "Waywalker");

	sf::Text message;

	Character pc = Character("Hans", "Human", 3);

	Map map = Map("testMap");

	sf::Font font;
	font.loadFromFile("Resources\\Fonts\\CourierNew.ttf");

	message.setFont(font);

	string text = "Name: " + pc.getName() + "\nMove Speed: " + to_string(pc.getWalkSpeed());

	for (int y = 0; y < map.getTerrain().size(); y++) {
		
		for (int x = 0; x < map.getTerrain()[y].size(); x++) {
			text += to_string(map.getTerrain()[y][x]);
		}
		text += '\n';
	}

	message.setString(text);

	message.setCharacterSize(36);

	message.setFillColor(sf::Color::White);

	/* Load a music to play
	sf::Music music;
	music.openFromFile("Village Consort.mp3");
	// Play the music
	music.play();
	*/
	while (window.isOpen())
	{
		sf::Event event;
		while (window.pollEvent(event))
		{
			if (event.type == sf::Event::Closed)
				window.close();
		}

		// Clear everything from the last run of the while loop
		window.clear();

		// Draw our message
		window.draw(message);

		// Draw our game scene here
		// Just a message for now

		// Show everything we just drew
		window.display();
	}

	return 0;
}

