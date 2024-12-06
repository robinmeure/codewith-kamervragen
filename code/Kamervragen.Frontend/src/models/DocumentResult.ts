export interface IDocumentResult {
id:string,
title:string,
subject:string,
reference:string,
date:string,
questions_and_answers:IQuestions_And_Answers[]
}

export interface IQuestions_And_Answers {
question:string,
answer?:string
}
