# Bowling Game

Keeps track of score, frames and current state of a standard [Ten-pin bowling](https://en.wikipedia.org/wiki/Ten-pin_bowling) game for a single player.

## API design

The API uses immutable datastructures and ["poor-mans" discriminated unions](https://spencerfarley.com/2021/03/26/unions-in-csharp/).
This enables "time travelling" or just simply undoing bowling rolls, by keeping a hold of previous `Game` values.

The API throws exceptions when the domain logic is violated. For instance when rolling more than 10 pins in a standard frame.

## Usage

### New game
```c#
var game = TC.Bowling.Game.New(); // creates a fresh game
```

### Inspecting the state of a game

`game.State` returns a value of [`GameState`](./TC.Bowling.Domain/GameState.cs).

### Getting the score of a game

`game.Score()` returns the current score of the game.

### Registering a roll

`game.Roll(numberOfPins)` will return a new [`Game`](./TC.Bowling.Domain/Game.cs) value, when it is a valid roll (exception otherwise);

Rolls can be registered until the game is completed (after finalizing the 10th frame), taking anywhere from 12 to 20 rolls to accomplish.