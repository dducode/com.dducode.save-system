# Changelog

## [1.4.0] - 2023-04-22

### Added

* Added asynchronous data handling:
    * Added new interface - the IPersistentObjectAsync for
      asynchronous handling of objects
    * Expanded UnityWriter and UnityReader handlers - added
      asynchronous methods in them to write and read data
    * Expanded the DataManager - added asynchronous methods
      to save and load objects

### Changed

* Removed write and read meshes array methods
* Removed methods for writing and reading a list of objects.
  Instead, use the write and read methods of an array of objects.
* Removed methods for saving and loading a list of objects from the DataManager.
  Instead, use the save and load methods of an array of objects.

## [1.3.0] - 2023-04-17

### Added

* Added write and read mesh and meshes array methods
* Added methods for writing and reading arrays of
  basic data types and unity structures

### Changed

* Removed methods for writing and reading MonoBehaviour classes.
  Use SaveObject method instead and implement the
  IPersistentObject interface in your MonoBehaviour classes

## [1.2.0] - 2023-04-15

### Added

* Added methods for writing and reading unity
  structures such as Color32, Matrix4X4, Vector2, Vector4.
* Added methods for writing and reading classes
  which inherits from MonoBehaviour

### Changed

* Renamed "ReadPosition" method to "ReadVector3"
  (in UnityReader class)

### Fixed

* Fixed problem with writing data to file. UnityWriter
  was leaving "trash" bytes at the end of the file if it wrote
  fewer bytes than the file contains

## [1.1.1] - 2023-04-14

### Added

* Added displaying of size of data
  (in "Data Manager / Get Data Size" menu in editor)

## [1.1.0] - 2023-04-14

### Added

* Added support of saving single object and list
  of objects

## [1.0.0] - 2023-04-13

### Added

* Create package