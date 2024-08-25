﻿using UnityEngine;

public class NN : MonoBehaviour
{
    int [] networkShape = {5, 4, 3, 2};
    public Layer [] layers;

    public void Awake()
    {
        layers = new Layer[networkShape.Length - 1];

        for(int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i+1]);
        }

        //This ensures that the random numbers we generate aren't the same pattern each time. 
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    //This function is used to feed forward the inputs through the network, and return the output, which is the decision of the network, in this case, the direction to move in.
    public float[] Brain(float [] inputs)
    {
        for(int i = 0; i < layers.Length; i++)
        {
            if(i == 0)
            {
                layers[i].Forward(inputs);
                layers[i].Activation();
            } 
            else if(i == layers.Length - 1)
            {
                layers[i].Forward(layers[i - 1].nodeArray);
            }
            else
            {
                layers[i].Forward(layers[i - 1].nodeArray);
                layers[i].Activation();
            }    
        }

        return(layers[layers.Length - 1].nodeArray);
    }

    //This function is used to copy the weights and biases from one neural network to another.
    public Layer[] copyLayers()
    {
        Layer[] tmpLayers = new Layer[networkShape.Length - 1];
        for(int i = 0; i < layers.Length; i++)
        {
            tmpLayers[i] = new Layer(networkShape[i], networkShape[i+1]);
            System.Array.Copy (layers[i].weightsArray, tmpLayers[i].weightsArray, layers[i].weightsArray.GetLength(0)*layers[i].weightsArray.GetLength(1));
            System.Array.Copy (layers[i].biasesArray, tmpLayers[i].biasesArray, layers[i].biasesArray.GetLength(0));
        }
        return(tmpLayers);
    }

    public class Layer
    {
        //attributes, variables and properties of the class Layer
        public float[,] weightsArray;
        public float[] biasesArray;
        public float [] nodeArray;

        public int n_inputs;
        public int n_neurons;

        //constructor, this is called when we create a new layer, it sets the number of inputs and nodes, and creates the arrays.
        public Layer(int n_inputs, int n_neurons)
        {
            this.n_inputs = n_inputs;
            this.n_neurons = n_neurons;

            weightsArray = new float [n_neurons, n_inputs];
            biasesArray = new float[n_neurons];
        }

        //forward pass, takes in an array of inputs and returns an array of outputs, which is then used as the input for the next layer, and so on, until we get to the output layer, which is returned as the output of the network.
        public void Forward(float [] inputsArray)
        {
            nodeArray = new float [n_neurons];

            for(int i = 0;i < n_neurons ; i++)
            {
                //sum of weights times inputs
                for(int j = 0; j < n_inputs; j++)
                {
                    nodeArray[i] += weightsArray[i,j] * inputsArray[j];
                }

                //add the bias
                nodeArray[i] += biasesArray[i];
            }
        }

        //This function is the activation function for the neural network uncomment the one you want to use.
        public void Activation()
        {
            for(int i = 0; i < nodeArray.Length; i++)
            {
                nodeArray[i] = (float)System.Math.Tanh(nodeArray[i]);
            }
        }

        float biasLimit = 1.0f; // Limits biases to the range [-1.0, 1.0]
        float weightLimit = 1.0f; // Limits weights to the range [-1.0, 1.0]

        public void MutateLayer(float mutationChance, float mutationAmount)
        {
            for (int i = 0; i < n_neurons; i++)
            {
                for (int j = 0; j < n_inputs; j++)
                {
                    if (Random.value < mutationChance)
                    {
                        float mutation = GaussianRandom(0, mutationAmount);
                        weightsArray[i, j] = Mathf.Clamp(weightsArray[i, j] + mutation, -weightLimit, weightLimit);
                    }
                }

                if (Random.value < mutationChance)
                {
                    float mutation = GaussianRandom(0, mutationAmount);
                    biasesArray[i] = Mathf.Clamp(biasesArray[i] + mutation, -biasLimit, biasLimit);
                }
            }
        }

        // Gaussian distribution with mean = 0 and adjustable standard deviation
        private float GaussianRandom(float mean, float stddev)
        {
            float u1 = Random.value;
            float u2 = Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stddev * randStdNormal;
        }
    }


    //Call the randomness function for each layer in the network.
    public void MutateNetwork(float mutationChance, float mutationAmount)
    {
        for(int i = 0; i < layers.Length; i++)
        {
            layers[i].MutateLayer(mutationChance, mutationAmount);
        }
    }
}