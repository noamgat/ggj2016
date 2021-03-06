// Canvas setup
var canvas = document.getElementById("canvas");
var ctx = canvas.getContext("2d");
w = $(canvas).width();
h = $(canvas).height();
// Flip the canvas so the y axis starts bottom left like it does in our
// unity project 
ctx.transform(1, 0, 0, -1, 0, canvas.height);

// Current selected tool and settings 
var tool = "line"; 
var drawing = false; 
var preview = {start: [0, 0], end: [0, 0]}
pointSize = 4; 
gridSize = 0.05;
snapping = true;
// Current loaded map
var points = [];
var edges  = [];
// Let's allow for undo history, totally for ggj scope
var stepHistory = [ {points:[], edges:[]} ];

// Tool selection
$(".editor-tools button").on("click", function(e) {
	$(".editor-tools button").removeClass("active");
	$target = $(e.currentTarget);
	$target.addClass("active");
	tool = $target.attr("data-tool");
})
$("#snapping-btn").on("click", function(e) {
	if (snapping) { 
		snapping = false;
		$("#snapping-btn").removeClass("btn-danger active");
	} else {
		snapping = true
		$("#snapping-btn").addClass("btn-danger active");
	}
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
    loadLevel(data)
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
$("#undo-btn").on("click", function(e) {
	undo();
})



startDrawing = function(e) {
	drawing = true; 
	updatePreview("start", e.offsetX, e.offsetY);
	$(canvas).on("mousemove", draw);
}
stopDrawing = function(e) {
	drawing = false; 
	$(canvas).off("mousemove", draw);
	
	if (e.offsetX) {
		updatePreview("end", e.offsetX, e.offsetY);
		if(!pointsEqual(preview.start, preview.end))
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
	if (snapping && (tool == "line" || tool == "circle")) {
		x = numberToGrid(x);
		y = numberToGrid(y);
	}
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

getPointInDist = function(minDist, p) {
	minDist = minDist || 0.025;
	for (i = 0; i < points.length - 1; i++) {
		if (distance(p, points[i]) <= minDist) 		
			return i;
	}
}
// Addition of the currently drawn shape into the list
// Shapes are independent of the grid on purpose
addShape_line = function() {
	distX = preview.end[0] - preview.start[0];
	distY = preview.end[1] - preview.start[1];
	steps = Math.ceil(distance(preview.start, preview.end) / gridSize);
	stepX = distX / steps;
	stepY = distY / steps;

	for (i = 0; i <= steps; i++) {
		x = preview.start[0] + stepX * i;
		y = preview.start[1] + stepY * i;
		points.push([x, y]);
	}
}
addShape_circle = function() {
	if (pointsEqual(preview.start, preview.end))
		return;
	a = generateCirlce(preview.start, preview.end);
	for (i = 0; i < a.length; i++) {
		points.push(a[i]);
	}
}
addShape_free = function() {}
addShape_remove = function() {}
addShape = function() {
	console.log("added shape");
	// Save current step
	addHistoryStep();

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

	dist = distance(p1, p2);
	step = Math.max(1-dist, 0.2);
	
	centerX = (p1[0] + p2[0]) / 2; 
	centerY = (p1[1] + p2[1]) / 2;

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
	if (pointsEqual(p1, p2))
		return;
	a = generateCirlce(p1, p2)
	for (var i = 0; i < a.length; i++) 
	    drawPoint(a[i], color);	
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

// Remove points
renderPreview_remove = function() {
	minDist = 0.025;
	// If the minimum distance is met, remove the point
	// We go on the reverse order in order to splice properly
	for (i = points.length - 1; i >= 0; i--) {
		if (distance(preview.end, points[i]) <= minDist) {		
			removePoint(i);
		}
	}
}
// Removing the point by splicing the array
// We temporarily throw the points out of the canvas for now
removePoint = function(i) {
	//points.splice(i, 1);
	points[i] = [-1.5, -1.5];
	removeSegmentByPoint(i);
}
// We then have to then remove any reference point in edges
removeSegmentByPoint = function (p) {
	for (i = edges.length - 1; i >= 0; i--) {
		if (edges[i][0] == p || edges[i][1] == p) {
			edges.splice(i, 1);
		}
	}
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
			"yellow"
		);
	}
	// Render preview 
	if (drawing)
		window["renderPreview_" + tool]()
}

loadLevel = function (data) {
    points = data.points; 
    edges = data.edges;
}

addHistoryStep = function() {
	data = {points: points.concat(), edges: edges.concat()}
	stepHistory.push(data);
}

undo = function () {
	if (!stepHistory.length)
		return ;
	data = stepHistory.pop();
	points = data.points;
	edges = data.edges;
	render();
}

distance = function (p1, p2) {
	return Math.sqrt( (p1[0]-p2[0]) * (p1[0]-p2[0]) + (p1[1]-p2[1]) * (p1[1]-p2[1]) );
}

pointsEqual = function (p1, p2) {
	if (p1[0] == p2[0] && p1[1] == p2[1])
		return true
	return false;
}


render();