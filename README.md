<p align="center">

![](images~/logo.png)

</p>

# Save System

See [docs](https://dducode.github.io/save-system-docs/)
to learn how to use the save system:

* See [manual](https://dducode.github.io/save-system-docs/manual/intro.html)
  for quick start
* See [this page](https://dducode.github.io/save-system-docs/manual/installing.html)
  to learn how to install the system
* See [Scripting API](https://dducode.github.io/save-system-docs/api/index.html)
  to more details of system usage
* See [Changelog](https://dducode.github.io/save-system-docs/changelog/CHANGELOG.html)
  to know latest changes in the system

## About Save System

The save system is a package for saving and loading game data.
This system allows you to save the state of the game at a some moment
(ex. before quitting) and restore it after entering.
You can choose one of the following ways to commit
the state of the game:

* Synchronous
* Asynchronous single threading mode (on the player loop)
* Asynchronous multithreading mode (on the thread pool)

In low level you can control unity structures such as Vector2,
Vector3, Vector4, Quaternion, Color, Color32, Matrix4X4, Mesh.

Also you can write down and read base data types and custom
classes and structures.