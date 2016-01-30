// Canvas setup
var canvas = document.getElementById("canvas");
var ctx = canvas.getContext("2d");
w = $(canvas).width();
h = $(canvas).height();
// Flip the canvas so the y axis starts bottom left like it does in our
// unity project 
ctx.transform(1, 0, 0, -1, 0, canvas.height);

// Current selected tool
var tool = "line"; 
var drawing = false; 
var preview = {start: [0, 0], end: [0, 0]}
// Current loaded map
var points = [];
var edges  = [];

pointSize = 4; 
gridSize = 0.05;

// Tool selection
$(".editor-tools button").on("click", function(e) {
	$(".editor-tools button").removeClass("active");
	$target = $(e.currentTarget);
	$target.addClass("active");
	tool = $target.attr("data-tool");
})
// Modal opening
$("#export-btn").on("click", function(e) {
	$('#export-modal').modal('show');   
	data = JSON.stringify({ points: points, edges: edges });
	$('#export-modal textarea').val(data.toString());
})
$("#import-btn").on("click", function(e) {
	$('#import-modal').modal('show');   
})
// Import
$('#import-modal .btn-primary').on("click", function(e) {
	content = $('#import-modal textarea').val();
	$('#import-modal').modal('hide');  
	try {
		data = JSON.parse(content);
    } catch (e) {
        return alert("error parsing data goldy!");
    }
    points = data.points; 
    edges = data.edges;
    render();
}) 
// Load image by url
$("#load-image-btn").on("click", function(e) {
	$('#load-image-modal').modal('show');
})
$("#load-image-modal .btn-primary").on("click", function(e) {
	url = $('#load-image-modal input').val();
	// Leave blank to hide the image. 
	if (!url) {
		$(".canvas-container img").hide();
	} else {
		$(".canvas-container img").attr('src', url);
		$(".canvas-container img").show();
	}
	$('#load-image-modal').modal('hide');
})

startDrawing = function(e) {
	drawing = true; 
	updatePreview("start", e.offsetX, e.offsetY);
	$(canvas).on("mousemove", draw);
}
stopDrawing = function(e) {
	drawing = false; 
	$(canvas).off("mousemove", draw);
	
	//
	if (e.offsetX) {
		updatePreview("end", e.offsetX, e.offsetY);
		addShape();
	}
	render();
}
cancelDrawing = function () {
	stopDrawing(false);
}
draw = function(e) {
	updatePreview("end", e.offsetX, e.offsetY);
	render();
}
updatePreview = function(type, x, y) {
	x = numberToGrid(x);
	y = numberToGrid(y);
	preview[type] = [
		transformCanvasToUnityX(x), 
		transformCanvasToUnityY(y)
	]
}

// Transform X canvas units into values from 0 to 1
transformCanvasToUnityX = function (x) {
	return x / w;
}
// Transform Y canvas units into values from 0 to 1
// We also flip the origin axis
transformCanvasToUnityY = function (y) {
	return (h - y) / h;
}
// Snap to grid. Assumes 1/1 ratio on canvas 
numberToGrid = function (n) {
	snap = w * gridSize;
	return Math.round(n / snap) * snap;
}

// Press down
$(canvas).on("mousedown", startDrawing);
// Press up 
$(canvas).on("mouseup", stopDrawing);
// Press up, outside of the canvas 
$(document).on("mouseup", function(e) {
	// Cancel the drawing if we release outside of the canvas
	if ($(e.target)[0] != $(canvas)[0])
		cancelDrawing();
});

// Addition of the currently drawn shape into the list
// Shapes are independent of the grid on purpose
addShape_line = function() {
	distX = preview.end[0] - preview.start[0];
	distY = preview.end[1] - preview.start[1];
	steps = Math.abs(distX) * (w * gridSize) + Math.abs(distY)  * (w * gridSize);
	
	stepX = distX / steps;
	stepY = distY / steps;
		
	for (i = 0; i <= steps; i++) {
		coordinates = [
			preview.start[0] + stepX * i,
			preview.start[1] + stepY * i
			];
		points.push(coordinates);
	}

}
addShape_circle = function() {
	a = generateCirlce(preview.start, preview.end);
	for (i = 0; i < a.length; i++) {
		points.push(a[i]);
	}
}
addShape_free = function() {}
addShape_remove = function() {}
addShape = function() {
	// Save the current amount of points, as we use it to connect the new generated
	// points later
	points_length = points.length;
	// Add the new points
	window["addShape_" + tool]();
	// Connect the points by generating edges between them. Dismiss the last point
	// as it does not connect to anything. 
	for (i = points_length; i < points.length - 1; i++) {
		edges.push([i, i+1]);
	}
	// Connect the first and last points if it's a circle. 
	if (tool == "circle") 
		edges.push([points_length, points.length-1]);
	
	render();
}

// Draw on canvas utils
drawPoint = function (p, color) {
	ctx.beginPath();
	ctx.strokeStyle = color;
	ctx.arc((p[0] * w), (p[1] * h), pointSize, 0, 2 * Math.PI);
	ctx.stroke();
}
drawLine = function (p1, p2, color) {
	ctx.beginPath();
	ctx.strokeStyle = color;
	ctx.moveTo(p1[0] * w, p1[1] * h);
	ctx.lineTo(p2[0] * w, p2[1] * h);
	ctx.stroke();
}
generateCirlce = function(p1, p2) {
	centerX = (p1[0] + p2[0]) / 2; 
	centerY = (p1[1] + p2[1]) / 2;

	dist = distance(p1, p2);;
	step = (1 - dist) / 2;

	radx = (p1[1] - p2[1]) / 2;
	rady = (p1[0] - p2[0]) / 2;

	arr = [];
	for (var i = 0 * Math.PI; i < 2 * Math.PI; i += step ) {
	    xPos = centerX - (radx * Math.sin(i)) * Math.sin(0 * Math.PI) + (rady * Math.cos(i)) * Math.cos(0 * Math.PI);
	    yPos = centerY + (rady * Math.cos(i)) * Math.sin(0 * Math.PI) + (radx * Math.sin(i)) * Math.cos(0 * Math.PI);
	    arr.push([xPos, yPos]);
	}
	return arr;
}
drawCircle = function (p1, p2, color) {
	a = generateCirlce(p1, p2)
	for (var i = 0; i < a.length; i++) 
	    drawPoint(a[i], color);	
}
// .. 
loadLevel = function(level) {
	//...
	render();
}

// Render previews
renderPreview_line = function() {
	drawLine(preview.start, preview.end, "green");
}
renderPreview_circle = function() {
	drawCircle(preview.start, preview.end, "green");
}
renderPreview_free = function() {
}
renderPreview_remove = function() {
}

render = function() {
	// Clear the canvas
	ctx.clearRect(0, 0, canvas.width, canvas.height);

	// Render grid
	for (i = 0; i < 1 / gridSize; i++) {
		drawLine([gridSize * i, 0], [gridSize * i, 1], "grey");
		drawLine([0, gridSize * i], [1, gridSize * i], "grey");
	}

	// Render points 
	for (i = 0; i < points.length; i++) {
		drawPoint(points[i], "blue");
	}
	// Render edges
	for (i = 0; i < edges.length; i++) {
		// Create the lines between the relevant points:
		// points[ what point? ] [x or y as in 0 or 1]
		// The point is figured within edges[looped i] [ first point 0 or second point 1 ]
		drawLine(
			[points[edges[i][0]][0], points[edges[i][0]][1]],
			[points[edges[i][1]][0], points[edges[i][1]][1]],
			"red"
		);
	}
	// Render preview 
	if (drawing)
		window["renderPreview_" + tool]()
}

distance = function (p1, p2) {
	return Math.sqrt( (p1[0]-p2[0]) * (p1[0]-p2[0]) + (p1[1]-p1[1]) * (p1[1]-p1[1]) );
}

