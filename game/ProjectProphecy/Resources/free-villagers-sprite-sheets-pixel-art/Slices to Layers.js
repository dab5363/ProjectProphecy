/*
--------Photoshop Script - Grid to Layers------------
Author: Oisin Conolly
        www.DigitalBiscuits.co.uk

This basic script will create new layers from your active layer, each equal in size according to the grid dimensions specified.
*/


//this is the size of our squares in pixels
var squareSize = 48;



var docRef = app.activeDocument;

//set the ruler type
if (app.preferences.rulerUnits != Units.PIXELS) {
  app.preferences.rulerUnits = Units.PIXELS;
}

var layerRef = docRef.activeLayer;
Flip();

function Flip() {
  for (y = 0; y < docRef.height; y += squareSize) {
    for (x = 0; x < docRef.width; x += squareSize) {
      //activate the original layer
      docRef.activeLayer = layerRef;
      //make the selection
      docRef.selection.select(Array(Array(x, y), Array(x, y + squareSize), Array(x + squareSize, y + squareSize), Array(x + squareSize, y)), SelectionType.REPLACE, 0, false);
      if (hasSelection()) {
        docRef.selection.resize(-100, undefined); // will flip layer horizontally
        // activeDocument.activeLayer.resize(undefined,-100); // will flip layer vertically
      }
    }
  }
}

function ToLayer() {
  for (y = 0; y < docRef.height; y += squareSize) {
    for (x = 0; x < docRef.width; x += squareSize) {
      //activate the original layer
      docRef.activeLayer = layerRef;
      //make the selection
      docRef.selection.select(Array(Array(x, y), Array(x, y + squareSize), Array(x + squareSize, y + squareSize), Array(x + squareSize, y)), SelectionType.REPLACE, 0, false);
      if (hasSelection()) {
        // copy the selection
        docRef.selection.copy();
        // create and paste new layer
        docRef.artLayers.add();
        docRef.paste();
      }
    }
  }
}

function hasSelection(doc) {
  if (doc == undefined) doc = app.activeDocument;
  try {
    app.displayDialogs = DialogModes.NO;
    doc.selection.copy();
    app.displayDialogs = DialogModes.YES;
    // executeAction(stringIDToTypeID("copyEvent"), undefined, DialogModes.NO); // Equals above
    return true;
  }
  catch (e) {
    return false;
  }
};