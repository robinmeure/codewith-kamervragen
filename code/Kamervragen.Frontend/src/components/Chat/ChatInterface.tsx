import { makeStyles, Toast, Toaster, ToastIntent, ToastTitle, tokens, useId, useToastController, Dialog, DialogTrigger, DialogSurface, Button, DialogBody, DialogTitle, DialogContent, DialogActions, Tag, TagGroup, TagGroupProps  } from '@fluentui/react-components';
import { useChatMessages } from '../../hooks/useChatMessages';
import { useState } from 'react';
import { MessageList } from './MessageList';
import { ChatInput } from './ChatInput';
import { ChatHeader } from './ChatHeader';
import { SelectedQAPair } from '../Search/QuestionAnswerList';
import { ISearchDocument } from '../../models/SearchDocument';
import SelectedDocumentsList from '../Search/SelectedDocumentList';
import { DocumentViewer } from '../Documents/DocumentViewer';
import { SelectedItemsPanel } from './SelectedItemsPanel';
import { TextCollapse20Filled } from '@fluentui/react-icons/fonts';
import { ExpandUpLeft20Filled } from '@fluentui/react-icons';
import { Stack } from '@fluentui/react';


const useClasses = makeStyles({
    root: {
        display: 'flex',
        width: "100%",
        paddingTop: tokens.spacingHorizontalM,
        flexDirection: 'column'
    },
    header: {
        height: "48px",
        display: "flex",
        flexDirection: 'column',
        paddingLeft: tokens.spacingVerticalM,
        paddingRight: tokens.spacingVerticalM,
        justifyContent: "center"
    },
    body: {
        display: 'flex',
        height: '100%',
        flexDirection: 'column',
    },
    toast: {
        width: '200%'
    },
    tags:
    {
        margin:'10px'
    },
    selectedQAContainer: {
        width: '70%',
        margin: 'auto',
    },
    qaStack: {
        width: '100%',
        justifyContent: 'center',
        marginTop: '20px'
    }
});

type chatInterfaceType = {
    selectedChatId: string | undefined,
    selectedDocuments: string[] | undefined,
    selectedQAPairs: SelectedQAPair[];
}

export function ChatInterface({ selectedChatId,selectedQAPairs }: chatInterfaceType) {
    const classes = useClasses();

    const { messages, sendMessage, chatPending, deleteMessages } = useChatMessages(selectedChatId);
    const [userInput, setUserInput] = useState<string>("");
    const [selectedTab, setSelectedTab] = useState<string>("chat");
    const toasterId = useId("toaster");
    const { dispatchToast } = useToastController(toasterId);
    const [isDialogVisible, setIsDialogVisible] = useState(false);
    const [selectedDocuments, setSelectedDocuments] = useState<ISearchDocument[]>([]);
    const [selectedQAPairsState, setSelectedQAPairsState] = useState<SelectedQAPair[]>(selectedQAPairs);
    const [isExpanded, setIsExpanded] = useState<boolean>(true);
    const [includeQA, setIncludeQA] = useState<boolean>(false);
    const [includeDocs, setIncludeDocs] = useState<boolean>(false);

    const toggleIncludeQA = () => {
        setIncludeQA(prev => !prev);
    };
    
      const toggleIncludeDocs = () => {
        setIncludeDocs(prev => !prev);
    };

    const notify = (intent:ToastIntent, notification:string) =>
        dispatchToast(
            <Toast className={classes.toast}>
                <ToastTitle>{notification}</ToastTitle>
            </Toast>,
            { position:'top-end', intent: intent, timeout: 5000 }
        );

        const submitMessage = async (message: string) => {
            if (selectedChatId && message) {
            setUserInput("");
            const selectedQAPairsToSubmit = includeQA ? selectedQAPairsState : [];
            const documentsToSubmit = includeDocs ? selectedDocuments : [];
            console.log(selectedQAPairsState);
            //const success = await sendMessage({ message, selectedQAPair: selectedQAPairsState });
            const success = await sendMessage({ 
                message, 
                selectedQAPair: selectedQAPairsToSubmit, 
                includeDocs: includeDocs,
                includeQA: includeQA 
              });
            if (!success) notify('error', "Failed to send message.");
        }
    }

    const handleFollowUp = (question: string) => {
        submitMessage(question);
    }

    const clearChat = async () => {
        setIsDialogVisible(true);
    }

    const confirmClearChat = async () => {
        setIsDialogVisible(false);
        const success = await deleteMessages();
        if (!success) notify('error', "Failed to clear chat.");
        else notify('success', "Chat cleared.");
    }

    const removeItem: TagGroupProps["onDismiss"] = (_e, { value }) => {
        setSelectedDocuments((prevSelectedDocuments) =>
          prevSelectedDocuments.filter((doc) => doc.documentId !== value)
        );
      };

    const onQASelected = (selectedPairs: SelectedQAPair[]) => {
        setSelectedQAPairsState(selectedPairs);
    }

    const toggleQAPairSelection = (question: string) => {
        setSelectedQAPairsState(prev => {
          const isSelected = prev.some(qa => qa.question === question);
          if (isSelected) {
            return prev.filter(qa => qa.question !== question);
          } else {
            // Find the QA pair from initialSelectedQAPairs and add it
            const qaToAdd = selectedQAPairs.find(qa => qa.question === question);
            return qaToAdd ? [...prev, qaToAdd] : prev;
          }
        });
      };

    return (
        <div className={classes.root}>
            <div className={classes.body}>
                <Toaster toasterId={toasterId} />
                {(selectedChatId) && (<ChatHeader selectedTab={selectedTab} setSelectedTab={setSelectedTab} />)}
                {(selectedTab === "chat" && selectedChatId) && (
                    <>
                         <Dialog open={isDialogVisible} onOpenChange={(event, data) => setIsDialogVisible(data.open)}>
                            <DialogSurface>
                                <DialogBody>
                                <DialogTitle>Clear Messages</DialogTitle>
                                <DialogContent>
                                    Are you sure you want to clear this thread?
                                </DialogContent>
                                <DialogActions>
                                    <DialogTrigger disableButtonEnhancement>
                                    <Button appearance="secondary">No</Button>
                                    </DialogTrigger>
                                    <Button appearance="primary" onClick={confirmClearChat}>Yes</Button>
                                </DialogActions>
                                </DialogBody>
                            </DialogSurface>
                            </Dialog>
                        <MessageList messages={messages} loading={chatPending} onFollowUp={handleFollowUp} selectedChatId={selectedChatId} onQASelected={onQASelected} />                        
                        
                        <Stack horizontalAlign='center' className={classes.qaStack}>
                            <Button onClick={() => setIsExpanded(!isExpanded)} appearance='primary' disabled={selectedQAPairsState.length === 0} aria-label="Toggle panel">Geselecteerde vraagstukken</Button>
                            {(isExpanded && selectedQAPairsState.length > 0) && (
                                <div className={classes.selectedQAContainer} >
                                <SelectedDocumentsList selectedQAPairs={selectedQAPairsState} toggleQAPairSelection={toggleQAPairSelection} />
                                </div>
                            )}
                        </Stack>
                        <ChatInput 
                            value={userInput} 
                            setValue={setUserInput} 
                            onSubmit={() => submitMessage(userInput)} 
                            clearChat={clearChat}  
                            includeQA={includeQA}
                            includeDocs={includeDocs}
                            toggleIncludeQA={toggleIncludeQA}
                            toggleIncludeDocs={toggleIncludeDocs}/>
                    </>
                )}
                {selectedTab === 'documents' && (
                    <><DocumentViewer chatId={selectedChatId} /></>
                )}
            </div>
        </div>
    );
};
export default ChatInterface;