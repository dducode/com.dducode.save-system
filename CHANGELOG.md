# Changelog

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

* Fixed problem with writing in file. UnityWriter 
was leaving "trash" bytes in end of file if it was 
writing less bytes than file contains

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