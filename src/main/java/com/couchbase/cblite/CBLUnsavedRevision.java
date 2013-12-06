package com.couchbase.cblite;

import com.couchbase.cblite.internal.InterfaceAudience;
import com.couchbase.cblite.util.Log;

import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class CBLUnsavedRevision extends CBLRevision {

    private Map<String, Object> properties;

    /**
     * Constructor
     */
    @InterfaceAudience.Private
    protected CBLUnsavedRevision(CBLDocument document, CBLSavedRevision parentRevision) {

        super(document);

        parentRevID = parentRevision.getId();

        Map<String, Object> parentRevisionProperties = parentRevision.getProperties();

        if (parentRevisionProperties == null) {
            properties = new HashMap<String, Object>();
            properties.put("_id", document.getId());
            properties.put("_rev", parentRevID);
        }
        else {
            properties = new HashMap<String, Object>(parentRevisionProperties);
        }

    }

    /**
     * Set whether this revision is a deletion or not (eg, marks doc as deleted)
     */
     @InterfaceAudience.Public
     public void setIsDeletion(boolean isDeletion) {
        if (isDeletion == true) {
            properties.put("_deleted", true);
        }
        else {
            properties.remove("_deleted");
        }
    }

    /**
     * Get the id of the owning document.  In the case of an unsaved revision, may return null.
     * @return
     */
    @Override
    @InterfaceAudience.Public
    public String getId() {
        if (properties != null) {
            return (String) properties.get("_id");
        }
        return null;
    }

    /**
     * Set the properties for this revision
     */
    @InterfaceAudience.Public
    public void setProperties(Map<String,Object> properties) {
        this.properties = properties;
    }

    /**
     * Saves the new revision to the database.
     *
     * This will throw an exception with a 412 error if its parent (the revision it was created from)
     * is not the current revision of the document.
     *
     * Afterwards you should use the returned CBLRevision instead of this object.
     *
     * @return A new CBLRevision representing the saved form of the revision.
     * @throws CBLiteException
     */
    @InterfaceAudience.Public
    public CBLSavedRevision save() throws CBLiteException {
        return document.putProperties(properties, parentRevID);
    }

    /**
     * Creates or updates an attachment.
     * The attachment data will be written to the database when the revision is saved.
     * @param attachment A newly-created CBLAttachment (not yet associated with any revision)
     * @param name The attachment name.
     */
    @InterfaceAudience.Public
    public void addAttachment(CBLAttachment attachment, String name) {
        Map<String, Object> attachments =  (Map<String, Object>) properties.get("_attachments");
        attachments.put(name, attachment);
        properties.put("_attachments", attachments);
        attachment.setName(name);
        attachment.setRevision(this);
    }

    /**
     * Deletes any existing attachment with the given name.
     * The attachment will be deleted from the database when the revision is saved.
     * @param name The attachment name.
     */
    @InterfaceAudience.Public
    public void deleteAttachment(String name) {
        addAttachment(null, name);
    }

    /**
     * Sets the userProperties of the Revision.
     * Set replaces all properties except for those with keys prefixed with '_'.
     */
    @InterfaceAudience.Public
    public void setUserProperties(Map<String,Object> userProperties) {
        Map<String, Object> newProps = new HashMap<String, Object>();
        newProps.putAll(userProperties);
        for (String key : properties.keySet()) {
            if (key.startsWith("_")) {
                newProps.put(key, properties.get(key));  // Preserve metadata properties
            }
        }
        properties = newProps;
    }

    /**
     * Sets the attachment with the given name. The Attachment data will be written to the Database when the Revision is saved.
     *
     * @param name The name of the Attachment to set.
     * @param contentType The content-type of the Attachment.
     * @param contentStream The Attachment content.  The InputStream will be closed after it is no longer needed.
     */
    @InterfaceAudience.Public
    public void setAttachment(String name, String contentType, InputStream contentStream) {
        CBLAttachment attachment = new CBLAttachment(contentStream, contentType);
        addAttachment(attachment, name);
    }

    /**
     * Sets the attachment with the given name. The Attachment data will be written to the Database when the Revision is saved.
     *
     * @param name The name of the Attachment to set.
     * @param contentType The content-type of the Attachment.
     * @param contentStreamURL The URL that contains the Attachment content.
     */
    @InterfaceAudience.Public
    public void setAttachment(String name, String contentType, URL contentStreamURL) {
        try {
            InputStream inputStream = contentStreamURL.openStream();
            setAttachment(name, contentType, inputStream);
        } catch (IOException e) {
            Log.e(CBLDatabase.TAG, "Error opening stream for url: " + contentStreamURL);
            throw new RuntimeException(e);
        }
    }


    @Override
    @InterfaceAudience.Public
    public Map<String, Object> getProperties() {
        return properties;
    }

    @Override
    @InterfaceAudience.Public
    public CBLSavedRevision getParentRevision() {
        if (parentRevID == null || parentRevID.length() == 0) {
            return null;
        }
        return document.getRevision(parentRevID);
    }

    @Override
    @InterfaceAudience.Public
    public String getParentRevisionId() {
        return parentRevID;
    }

    @Override
    @InterfaceAudience.Public
    public List<CBLSavedRevision> getRevisionHistory() throws CBLiteException {
        // (Don't include self in the array, because this revision doesn't really exist yet)
        CBLSavedRevision parent = getParentRevision();
        return parent != null ? parent.getRevisionHistory() : new ArrayList<CBLSavedRevision>();
    }



}
