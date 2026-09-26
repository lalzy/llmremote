// ==== Model List ===
const modelList = document.getElementById("modelList");
const emptyText = document.getElementById("emptyText");

function addItemButton(buttonText, func){
    const button = document.createElement("button");
    button.textContent = buttonText;
    button.addEventListener("click", func);
    return button;
}

function createModelItems(models){
    for(const model of models){
	const item = document.createElement("li");
	item.textContent = `${model.name}`;

	item.addEventListener("click", async () => {
	    const response = await fetch(`/api/llama/start`, {
		method:"POST",
		headers: {"Content-Type":"application/json"},
		body: JSON.stringify(model)
	    });
	});

	const editButton = addItemButton("Edit", () => openEdit(model));
	const deleteButton = addItemButton("Delete", () => openDelete(model));
	item.append(editButton, deleteButton)
	modelList.appendChild(item);
    }
}

async function loadModels(){
    const response = await fetch("/api/llmmodel/all?orderBy=Name");
    
    if (!response.ok) {
        console.error("Load failed:", response.status, await response.text());
        return;
    }
    
    modelList.innerHTML = "";
    
    const models = await response.json();

    createModelItems(models)
    


    emptyText.hidden = models.length > 0;
}

loadModels();

// ==== Add/Edit modal ====
const addModelButton = document.getElementById("addModel");
const modalDialog = document.getElementById("modalDialog");
let editingId = null;
const modelDialogTitle = document.getElementById("modelDialogTitle");

addModelButton.addEventListener("click", () => {
    editingId = null;
    modelDialogTitle.textContent = "Add Model";
    modelForm.reset();
    modalDialog.showModal();
});

function openEdit(model){
    editingId = model.id;
    modelDialogTitle.textContent = "Edit Model";
    document.getElementById("nameInput").value = model.name;
    document.getElementById("filePathInput").value = model.filePath;
    document.getElementById("contextInput").value = model.context ?? "";
    modalDialog.showModal();
}

// ==== Cancel Button ====
document.querySelectorAll(".cancel-button").forEach(button => {
    
    button.addEventListener("click", () => {
	button.closest("dialog").close();
    });
});

// ==== Delete ====
const deleteDialog = document.getElementById("deleteDialog");
const deleteName = document.getElementById("deleteName");
let deletingId = null;

function openDelete(model){
    deletingId = model.id;
    deleteName.textContent = model.name;
    deleteDialog.showModal();
}

document.getElementById("confirm-button").addEventListener("click", async () => {
    const response = await fetch(`api/llmmodel/${deletingId}`, {method: "DELETE"});
    if(!response.ok){
	console.Error("Delete failed:", response.status, await response.text());
	return;
    }

    deleteDialog.close();
    loadModels();
});

// ==== Submit ====

const modelForm = document.getElementById("modelForm");
const formError = document.getElementById("formError");

modelForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const model = {
	Name: document.getElementById("nameInput").value,
	FilePath: document.getElementById("filePathInput").value,
	Context: document.getElementById("contextInput").value || null,
    };
    
    const url = editingId === null ? "/api/llmmodel/add" : `/api/llmmodel/${editingId}`;
    const method = editingId === null ? "POST" : "PUT";
    
    const response = await fetch(url, {
	method:method,
	headers: {"Content-Type":"application/json"},
	body: JSON.stringify(model)
    });

    if(!response.ok){
	formError.textContent = "Could not save the model.";
	return;
    }

    formError.textContent = "";
    modelForm.reset();
    modalDialog.close();
    loadModels();
});
