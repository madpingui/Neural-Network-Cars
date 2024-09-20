# Neural Network Cars
In this project, I simulate around 20 cars per generation, each equipped with raycasts that give them "vision" by detecting distances to walls. The neural network behind each car uses this input to drive across a track. Over time, the cars evolve, adjusting their weights and biases through several generations to improve their navigation.

The system eliminates cars if they crash into a wall, stay still, or run out of fuel. The fitness function is based on how many checkpoints the cars pass, and the genetic algorithm incorporates elitism: the car that goes the furthest gets reproduced in the next generation. One car carries over the best-performing network, while the rest introduce slight mutations to their values, driving continuous improvement.

It’s fascinating to see how much potential there is in neural network systems. Even if it's a simple project i see a lot of improvements that can be done, like adding more hidden layers, using different activation functions, or fine-tuning the mutation rate could make the neural networks even more efficient. There are endless possibilities for increasing the performance and complexity of the cars behavior!

https://github.com/user-attachments/assets/c4c2ede9-6168-4a75-93b7-f76015da2539
