using System;

[Serializable]
public class NeuralNetwork
{
    private Layer[] layers; // Array of layers representing the neural network structure

    public NeuralNetwork(params int[] networkShape)
    {
        layers = new Layer[networkShape.Length - 1]; // Initialize layers based on the network shape

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i + 1]); // Create layers with input/output neurons
        }
    }

    // Processes inputs through each layer of the neural network
    public float[] ProcessInputs(float[] inputs)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            inputs = layers[i].Forward(inputs); // Forward propagation
            if (i < layers.Length - 1)
            {
                inputs = layers[i].Activation(inputs); // Apply activation function (e.g., tanh)
            }
        }

        return inputs; // Output the final result
    }

    // Deep copy of the neural network for reproducing and mutating new agents
    public NeuralNetwork DeepCopy()
    {
        int[] networkShape = new int[layers.Length + 1];
        networkShape[0] = layers[0].InputCount;

        for (int i = 0; i < layers.Length; i++)
        {
            networkShape[i + 1] = layers[i].NeuronCount;
        }

        NeuralNetwork copiedNetwork = new NeuralNetwork(networkShape);

        for (int i = 0; i < layers.Length; i++)
        {
            copiedNetwork.layers[i] = layers[i].DeepCopy();
        }

        return copiedNetwork;
    }

    // Mutates the network by randomly adjusting weights and biases based on mutation chance and amount
    public void Mutate(float mutationChance, float mutationAmount)
    {
        foreach (var layer in layers)
        {
            layer.Mutate(mutationChance, mutationAmount); // Mutate each layer's weights and biases
        }
    }
}

[Serializable]
public class Layer
{
    private float[,] weights;
    private float[] biases;

    public int InputCount { get; private set; }
    public int NeuronCount { get; private set; }

    public Layer(int inputCount, int neuronCount)
    {
        InputCount = inputCount;
        NeuronCount = neuronCount;

        weights = new float[neuronCount, inputCount]; // Initialize weights
        biases = new float[neuronCount]; // Initialize biases

        InitializeRandomly(); // Random initialization of weights and biases
    }

    // Random initialization of weights and biases
    private void InitializeRandomly()
    {
        for (int i = 0; i < NeuronCount; i++)
        {
            for (int j = 0; j < InputCount; j++)
            {
                weights[i, j] = UnityEngine.Random.Range(-1f, 1f); // Randomize weights
            }
            biases[i] = UnityEngine.Random.Range(-1f, 1f); // Randomize biases
        }
    }

    // Forward propagation: computes outputs from inputs and current weights
    public float[] Forward(float[] inputs)
    {
        float[] outputs = new float[NeuronCount];

        for (int i = 0; i < NeuronCount; i++)
        {
            float sum = 0;
            for (int j = 0; j < InputCount; j++)
            {
                sum += weights[i, j] * inputs[j]; // Weighted sum
            }
            outputs[i] = sum + biases[i]; // Apply bias
        }

        return outputs;
    }

    // Activation function (tanh)
    public float[] Activation(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = (float)Math.Tanh(inputs[i]); // Apply tanh activation
        }
        return inputs;
    }

    // Mutates the weights and biases based on chance and mutation amount
    public void Mutate(float mutationChance, float mutationAmount)
    {
        for (int i = 0; i < NeuronCount; i++)
        {
            for (int j = 0; j < InputCount; j++)
            {
                if (UnityEngine.Random.value < mutationChance)
                {
                    weights[i, j] += UnityEngine.Random.Range(-mutationAmount, mutationAmount); // Mutate weights
                }
            }

            if (UnityEngine.Random.value < mutationChance)
            {
                biases[i] += UnityEngine.Random.Range(-mutationAmount, mutationAmount); // Mutate biases
            }
        }
    }

    // Deep copy of the layer for reproducing networks
    public Layer DeepCopy()
    {
        Layer copiedLayer = new Layer(InputCount, NeuronCount);
        Array.Copy(weights, copiedLayer.weights, weights.Length); // Copy weights
        Array.Copy(biases, copiedLayer.biases, biases.Length); // Copy biases
        return copiedLayer;
    }
}
