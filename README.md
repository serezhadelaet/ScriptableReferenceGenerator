# Scriptable Reference Generator

A little but easy and handy solution to inject dependencies in the Unity-Way (Code & Editor via ScriptableObjects)

## How to use
Mark any of your MonoBehaviour with <code>[ReferenceAutoGeneration]</code> attribute</br>
After project compilation, add a fresh generated script called <code>*YourClassName*RefSetter</code> 
to GameObject which contains your <code>MonoBehaviour</code></br>
Then put to the <code>Scriptable</code> field your fresh generated asset from <code>Assets/Data/References/</code> folder.
The Generated scriptable asset holds an instance of an object.
</br>To use that instance you need to reference the asset in the way as you always reference assets:</br>
Via <code>[SerializedField]</code> or just mark it as <code>public</code> in your <code>MonoBehaviour</code>
</br>
An example of usage <a href="https://github.com/serezhadelaet/ScriptableReferenceGenerator/blob/master/Assets/Scripts/UsageExample.cs"> here</a>

## When to use
When you don't need all the power that Zenject provides, 
when you have not too many reference-depepending scripts, but you want a simple way to handle dependency injection with some benefits.

## What the benefits
You can create multiple assets by duplicate one of them, doing this you can hold a multiple instances of needed type of a class.</br>
For example you have few characters which needs to be referenced somewhere else,
then you can mark <code>Character</code> class with the <code>[ReferenceAutoGeneration]</code> attribute,
duplicate your generated asset, rename it and use this assets independently to link to your characters.</br>
It also works for tests in the same way.
Using this you don't need to hardcode references, using Singletone/ServiceLocator, or doing any other tricky ways to achieve some sort of dependency injection.
